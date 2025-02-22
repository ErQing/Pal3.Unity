﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Renderer
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Threading;
    using Core.DataLoader;
    using Core.DataReader.Mv3;
    using Core.GameBox;
    using Core.Renderer;
    using Core.Utils;
    using Dev;
    using UnityEngine;

    /// <summary>
    /// MV3(.mv3) model renderer
    /// </summary>
    public class Mv3ModelRenderer : MonoBehaviour
    {
        public event EventHandler<int> AnimationLoopPointReached;

        private const string MV3_ANIMATION_HOLD_EVENT_NAME = "hold";
        private const string MV3_MODEL_DEFAULT_TEXTURE_EXTENSION = ".tga";
        private const float TIME_TO_TICK_SCALE = 5000f;

        private ITextureResourceProvider _textureProvider;
        private VertexAnimationKeyFrame[] _keyFrames;
        private Texture2D _texture;
        private Material _material;
        private GameBoxMaterial _gbMaterial;
        private Mv3AnimationEvent[] _events;
        private bool _textureHasAlphaChannel;
        private Color _tintColor;
        private GameObject _meshObject;
        private StaticMeshRenderer _meshRenderer;
        private Coroutine _animation;
        private string _animationName;
        private CancellationTokenSource _animationCts;

        private bool _isActionInHoldState;
        private uint _actionHoldingTick;

        private Mesh _renderMesh;
        private Vector3[] _vertexBuffer;
        private int[] _triangleBuffer;

        public void Init(Mv3Mesh mv3Mesh,
            Mv3Material material,
            Mv3AnimationEvent[] events,
            VertexAnimationKeyFrame[] keyFrames,
            ITextureResourceProvider textureProvider,
            Color tintColor)
        {
            _tintColor = tintColor;
            _gbMaterial = material.Material;
            _textureProvider = textureProvider;
            _events = events;
            _keyFrames = keyFrames;
            _texture = textureProvider.GetTexture(material.TextureNames[0], out var hasAlphaChannel);
            _textureHasAlphaChannel = hasAlphaChannel;
            _animationName = mv3Mesh.Name;
        }

        public void PlayAnimation(int loopCount = -1, float fps = -1f)
        {
            if (_keyFrames == null || _keyFrames.Length == 0)
            {
                throw new Exception("Animation not initialized.");
            }

            StopAnimation();

            if (_meshRenderer == null)
            {
                _meshObject = new GameObject(_animationName);

                // Attach BlendFlag and GameBoxMaterial to the GameObject for better debuggability
                #if UNITY_EDITOR
                var materialInfoPresenter = _meshObject.AddComponent<MaterialInfoPresenter>();
                materialInfoPresenter.blendFlag = (uint) (_textureHasAlphaChannel ? 1 : 0);
                materialInfoPresenter.material = _gbMaterial;
                #endif

                _meshRenderer = _meshObject.AddComponent<StaticMeshRenderer>();

                _material = new Material(Shader.Find("Pal3/StandardNoShadow"));
                _material.SetTexture(Shader.PropertyToID("_MainTex"), _texture);

                var cutoff = _textureHasAlphaChannel ? 0.3f : 0f;
                if (cutoff > Mathf.Epsilon)
                {
                    _material.SetFloat(Shader.PropertyToID("_Cutoff"), cutoff);
                }

                _material.SetColor(Shader.PropertyToID("_TintColor"), _tintColor);

                _vertexBuffer = _keyFrames[0].Vertices;
                _triangleBuffer = _keyFrames[0].Triangles;

                _renderMesh = _meshRenderer.Render(_vertexBuffer,
                    _triangleBuffer,
                    Array.Empty<Vector3>(),
                    _keyFrames[0].Uv,
                    _material,
                    true);

                _meshRenderer.RecalculateBoundsNormalsAndTangents();

                _meshObject.transform.SetParent(transform, false);
            }

            if (_animation != null) StopCoroutine(_animation);

            _animationCts = new CancellationTokenSource();
            _animation = StartCoroutine(PlayAnimationInternal(loopCount,
                fps,
                _animationCts.Token));
        }

        public void ChangeTexture(string textureName)
        {
            if (!textureName.Contains(".")) textureName += MV3_MODEL_DEFAULT_TEXTURE_EXTENSION;
            _texture = _textureProvider.GetTexture(textureName);
            _material.SetTexture(Shader.PropertyToID("_MainTex"), _texture);
        }

        public void StopAnimation()
        {
            if (_animation != null)
            {
                _animationCts.Cancel();
                StopCoroutine(_animation);
                _animation = null;
            }
        }

        public Bounds GetBounds()
        {
            return _meshRenderer.GetRendererBounds();
        }

        public Bounds GetLocalBounds()
        {
            return _meshRenderer.GetMeshBounds();
        }

        public bool IsActionInHoldState()
        {
            return _isActionInHoldState;
        }

        public void ResumeAction()
        {
            StartCoroutine(ResumeActionInternal());
        }

        private IEnumerator ResumeActionInternal(float fps = -1f)
        {
            if (!_isActionInHoldState) yield break;
            var endTick = _keyFrames.Last().Tick;
            _animationCts = new CancellationTokenSource();
            yield return PlayOneTimeAnimationInternal(_actionHoldingTick, endTick, fps, _animationCts.Token);
            _isActionInHoldState = false;
            _actionHoldingTick = 0;
            AnimationLoopPointReached?.Invoke(this, -2);
        }

        private IEnumerator PlayAnimationInternal(int loopCount,
            float fps,
            CancellationToken cancellationToken)
        {
            uint startTick = 0;
            uint endTick = _keyFrames.Last().Tick;

            if (loopCount == -1)
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    yield return PlayOneTimeAnimationInternal(startTick, endTick, fps, cancellationToken);
                    AnimationLoopPointReached?.Invoke(this, loopCount);
                }
            }
            else if (loopCount > 0)
            {
                while (!cancellationToken.IsCancellationRequested && --loopCount >= 0)
                {
                    yield return PlayOneTimeAnimationInternal(startTick, endTick, fps, cancellationToken);
                    AnimationLoopPointReached?.Invoke(this, loopCount);
                }
            }
            else if (loopCount == -2)
            {
                _actionHoldingTick = _events.LastOrDefault(e =>
                    e.Name.Equals(MV3_ANIMATION_HOLD_EVENT_NAME, StringComparison.OrdinalIgnoreCase)).Tick;
                if (_actionHoldingTick != 0)
                {
                    yield return PlayOneTimeAnimationInternal(startTick, _actionHoldingTick, fps, cancellationToken);
                    _isActionInHoldState = true;
                }
                else
                {
                    yield return PlayOneTimeAnimationInternal(startTick, endTick, fps, cancellationToken);
                }
                AnimationLoopPointReached?.Invoke(this, loopCount);
            }
        }

        private IEnumerator PlayOneTimeAnimationInternal(uint startTick,
            uint endTick,
            float fps,
            CancellationToken cancellationToken)
        {
            var startTime = Time.timeSinceLevelLoad;
            var frameTicks = _keyFrames.Select(f => f.Tick).ToArray();

            while (!cancellationToken.IsCancellationRequested)
            {
                var tick = ((Time.timeSinceLevelLoad - startTime) * TIME_TO_TICK_SCALE
                                  + startTick);

                if (tick >= endTick)
                {
                    yield break;
                }

                var currentFrameIndex = Utility.GetFloorIndex(frameTicks, (uint)tick);

                var influence = (tick - frameTicks[currentFrameIndex]) /
                                (frameTicks[currentFrameIndex + 1] - frameTicks[currentFrameIndex]);

                for (var i = 0; i < _vertexBuffer.Length; i++)
                {
                    _vertexBuffer[i] = Vector3.Lerp(_keyFrames[currentFrameIndex].Vertices[i],
                        _keyFrames[currentFrameIndex + 1].Vertices[i], influence);
                }

                _renderMesh.vertices = _vertexBuffer;
                //_meshRenderer.RecalculateBoundsNormalsAndTangents();

                if (fps <= 0)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(1 / fps);
                }
            }
        }

        private void OnDisable()
        {
            Dispose();
        }

        private void Dispose()
        {
            StopAnimation();

            if (_renderMesh != null)
            {
                Destroy(_renderMesh);
            }

            if (_meshRenderer != null)
            {
                Destroy(_meshRenderer);
            }

            if (_meshObject != null)
            {
                Destroy(_meshObject);
            }
        }
    }
}