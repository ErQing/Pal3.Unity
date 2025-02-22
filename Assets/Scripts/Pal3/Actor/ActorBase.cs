// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Actor
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.DataLoader;
    using Core.DataReader.Cpk;
    using Core.DataReader.Ini;
    using Core.DataReader.Mv3;
    using Data;
    using MetaData;
    using UnityEngine;

    public enum ActorAnimationFileType
    {
        Mv3 = 0,
        Mov
    }

    public abstract class ActorBase
    {
        public ActorAnimationFileType AnimationFileType;

        private ActorConfigFile _actorConfig;

        private readonly GameResourceProvider _resourceProvider;

        private string _actorName;
        private readonly Dictionary<string, (Mv3File mv3File, ITextureResourceProvider textureProvider)> _actionCache = new ();

        protected ActorBase(GameResourceProvider resourceProvider, string name)
        {
            _actorName = name;
            _resourceProvider = resourceProvider;

            InitActorConfig(name);
        }

        private void InitActorConfig(string name)
        {
            var separator = CpkConstants.CpkDirectorySeparatorChar;

            var defaultActorConfigFile = $"{FileConstants.BaseDataCpkPathInfo.cpkName}{separator}" +
                                         $"{FileConstants.ActorFolderName}{separator}{name}{separator}{name}.ini";

            if (_resourceProvider.GetActorConfig(defaultActorConfigFile) is { } defaultConfig)
            {
                _actorConfig = defaultConfig;
                AnimationFileType = ActorAnimationFileType.Mv3;
            }
            else // For MOV animation actor, config files are defined as 01.ini, 02.ini ...up tp 09.ini
            {
                Debug.LogWarning($"Unsupported MOV actor config found for: actor {name}");
                //throw new NotImplementedException("MOV actor currently not supported.");
                AnimationFileType = ActorAnimationFileType.Mov;
            }
        }

        protected void ChangeName(string name)
        {
            _actorName = name;
            _actionCache.Clear();
            InitActorConfig(name);
        }

        public (Mv3File mv3File, ITextureResourceProvider textureProvider) GetActionMv3(ActorActionType actorActionType)
        {
            return GetActionMv3(ActorConstants.ActionNames[actorActionType]);
        }

        public bool HasAction(string actionName)
        {
            if (_actorConfig == null) return false;

            return _actorConfig.ActorActions
                .Any(act => act.ActionName.ToLower().Equals(actionName));
        }

        public (Mv3File mv3File, ITextureResourceProvider textureProvider) GetActionMv3(string actionName)
        {
            actionName = actionName.ToLower();

            if (_actionCache.ContainsKey(actionName)) return _actionCache[actionName];

            var action = _actorConfig.ActorActions
                .First(act => act.ActionName.ToLower().Equals(actionName));

            var separator = CpkConstants.CpkDirectorySeparatorChar;

            var mv3File = $"{FileConstants.BaseDataCpkPathInfo.cpkName}{separator}" +
                          $"{FileConstants.ActorFolderName}{separator}{_actorName}{separator}" +
                          $"{action.ActionFileName}";

            _actionCache[actionName] = _resourceProvider.GetMv3(mv3File);

            return _actionCache[actionName];
        }
    }
}