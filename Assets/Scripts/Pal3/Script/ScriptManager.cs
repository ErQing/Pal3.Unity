// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Script
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Command;
    using Command.InternalCommands;
    using Command.SceCommands;
    using Core.DataReader.Sce;
    using Data;
    using MetaData;
    using Newtonsoft.Json;
    using UnityEngine;

    public class ScriptManager :
        ICommandExecutor<ScriptRunCommand>,
        ICommandExecutor<ScriptVarSetValueCommand>,
        ICommandExecutor<ResetGameStateCommand>
    {
        private readonly Dictionary<int, int> _globalVariables = new ();

        private readonly SceFile _systemSceFile;
        private readonly SceFile _bigMapSceFile;
        private SceFile _sceFile;
        private readonly Queue<PalScriptRunner> _pendingScripts = new ();
        private readonly List<PalScriptRunner> _runningScripts = new ();

        public ScriptManager(GameResourceProvider resourceProvider)
        {
            _systemSceFile = resourceProvider.GetSystemSce();
            _bigMapSceFile = resourceProvider.GetBigMapSce();
            CommandExecutorRegistry<ICommand>.Instance.Register(this);
        }

        public void Dispose()
        {
            CommandExecutorRegistry<ICommand>.Instance.UnRegister(this);

            foreach (var scriptRunner in _runningScripts)
            {
                scriptRunner.OnCommandExecutionRequested -= OnCommandExecutionRequested;
                scriptRunner.Dispose();
            }
        }

        public void SetGlobalVariable(int variable, int value)
        {
            _globalVariables[variable] = value;
        }

        public Dictionary<int, int> GetGlobalVariables()
        {
            return _globalVariables;
        }

        public int GetNumberOfRunningScripts()
        {
            return _pendingScripts.Count + _runningScripts.Count;
        }

        public bool AddScript(uint scriptId, bool isBigMapScript = false)
        {
            if (scriptId == ScriptConstants.InvalidScriptId) return false;

            if (_pendingScripts.Any(s => s.ScriptId == scriptId) ||
                _runningScripts.Any(s => s.ScriptId == scriptId))
            {
                Debug.LogError($"Script is already running: {scriptId}");
                return false;
            }

            PalScriptRunner scriptRunner;
            if (isBigMapScript)
            {
                Debug.Log($"Add BigMap script id: {scriptId}");
                scriptRunner = PalScriptRunner.Create(_bigMapSceFile, scriptId, _globalVariables);
            }
            else if (scriptId <= ScriptConstants.SystemScriptIdMax)
            {
                Debug.Log($"Add system script id: {scriptId}");
                scriptRunner = PalScriptRunner.Create(_systemSceFile, scriptId, _globalVariables);
            }
            else
            {
                Debug.Log($"Add scene script id: {scriptId}");
                scriptRunner = PalScriptRunner.Create(_sceFile, scriptId, _globalVariables);
            }

            scriptRunner.OnCommandExecutionRequested += OnCommandExecutionRequested;
            _pendingScripts.Enqueue(scriptRunner);
            return true;
        }

        private void OnCommandExecutionRequested(object sender, ICommand command)
        {
            var sceCommandId = command.GetType().GetCustomAttribute<SceCommandAttribute>()?.Id;

            Debug.Log($"Script {((PalScriptRunner)sender).ScriptId} executing command: [{sceCommandId}] " +
                      $"{command.GetType().Name.Replace("Command", "")} " +
                      $"{JsonConvert.SerializeObject(command)}");

            CommandDispatcher<ICommand>.Instance.Dispatch(command);
        }

        public void Update(float deltaTime)
        {
            while (_pendingScripts.Count > 0)
            {
                _runningScripts.Add( _pendingScripts.Dequeue());
            }

            if (_runningScripts.Count == 0) return;

            var finishedScripts = _runningScripts.Where(script => !script.Update(deltaTime)).ToList();

            foreach (var finishedScript in finishedScripts)
            {
                _runningScripts.Remove(finishedScript);
                finishedScript.OnCommandExecutionRequested -= OnCommandExecutionRequested;
                finishedScript.Dispose();
            }

            foreach (var finishedScript in finishedScripts)
            {
                Debug.Log($"Script {finishedScript.ScriptId} finished running.");
                CommandDispatcher<ICommand>.Instance.Dispatch(
                    new ScriptFinishedRunningNotification(finishedScript.ScriptId));
            }
        }

        public void SetSceneScript(SceFile sceFile, string sceneScriptDescription)
        {
            _sceFile = sceFile;

            foreach (var scriptBlock in _sceFile.ScriptBlocks
                         .Where(scriptBlock =>
                             string.Equals(scriptBlock.Value.Description,
                                 sceneScriptDescription,
                                 StringComparison.OrdinalIgnoreCase)))
            {
                AddScript(scriptBlock.Key);
                break;
            }
        }

        public void Execute(ScriptRunCommand command)
        {
            if (!AddScript((uint) command.ScriptId))
            {
                CommandDispatcher<ICommand>.Instance.Dispatch(new ScriptFailedToRunNotification((uint) command.ScriptId));
            }
        }

        public void Execute(ScriptVarSetValueCommand command)
        {
            if (command.Variable < 0)
            {
                Debug.LogWarning($"Set global var {command.Variable} with value: {command.Value}");
                SetGlobalVariable(command.Variable, command.Value);
            }
        }

        public void Execute(ResetGameStateCommand command)
        {
            foreach (var script in _pendingScripts)
            {
                script.OnCommandExecutionRequested -= OnCommandExecutionRequested;
                script.Dispose();
            }

            foreach (var script in _runningScripts)
            {
                script.OnCommandExecutionRequested -= OnCommandExecutionRequested;
                script.Dispose();
            }

            _globalVariables.Clear();
        }
    }
}