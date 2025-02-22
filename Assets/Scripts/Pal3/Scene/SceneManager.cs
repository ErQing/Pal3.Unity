// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Scene
{
    using System.Diagnostics;
    using Command;
    using Command.InternalCommands;
    using Command.SceCommands;
    using Core.DataReader.Cpk;
    using Data;
    using MetaData;
    using Newtonsoft.Json;
    using Script;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class SceneManager :
        ICommandExecutor<SceneLoadCommand>,
        ICommandExecutor<ResetGameStateCommand>
    {
        private readonly Camera _mainCamera;
        private readonly GameResourceProvider _resourceProvider;
        private readonly ScriptManager _scriptManager;

        private GameObject _currentSceneRoot;
        private Scene _currentScene;

        public SceneManager(GameResourceProvider resourceProvider, ScriptManager scriptManager, Camera mainCamera)
        {
            _resourceProvider = resourceProvider;
            _scriptManager = scriptManager;
            _mainCamera = mainCamera;
            CommandExecutorRegistry<ICommand>.Instance.Register(this);
        }

        public void Dispose()
        {
            DisposeCurrentScene();
            CommandExecutorRegistry<ICommand>.Instance.UnRegister(this);
        }

        public Scene GetCurrentScene()
        {
            return _currentScene;
        }

        public void LoadScene(string sceneFileName, string sceneName)
        {
            var timer = new Stopwatch();
            timer.Start();
            DisposeCurrentScene();

            var scnFile = _resourceProvider.GetScn(sceneFileName, sceneName);

            CommandDispatcher<ICommand>.Instance.Dispatch(new ScenePreLoadingNotification(scnFile.SceneInfo));
            Debug.Log("Loading scene: " + JsonConvert.SerializeObject(scnFile.SceneInfo));

            // Set current scene script file
            var sceFile = _resourceProvider.GetSceneSce(sceneFileName);
            _scriptManager.SetSceneScript(sceFile, $"_{sceneFileName}_{sceneName}");

            _currentSceneRoot = new GameObject($"Scene_{sceneFileName}_{sceneName}");
            _currentSceneRoot.transform.SetParent(null);
            _currentScene = _currentSceneRoot.AddComponent<Scene>();
            _currentScene.Init(_resourceProvider, _mainCamera);
            _currentScene.Load(scnFile, _currentSceneRoot);

            CommandDispatcher<ICommand>.Instance.Dispatch(new ScenePostLoadingNotification(scnFile.SceneInfo));
            Debug.Log($"Scene loaded in {timer.Elapsed.TotalSeconds} seconds.");
        }

        private void DisposeCurrentScene()
        {
            if (_currentScene != null)
            {
                Object.Destroy(_currentScene);
                Object.Destroy(_currentSceneRoot);
            }
        }

        public void Execute(SceneLoadCommand command)
        {
            LoadScene(command.SceneFileName, command.SceneName);
        }

        public void Execute(ResetGameStateCommand command)
        {
            DisposeCurrentScene();
        }
    }
}