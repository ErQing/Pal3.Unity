﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Command.SceCommands
{
    [SceCommand(136, "角色淡入")]
    public class ActorFadeInCommand : ICommand
    {
        public ActorFadeInCommand(int actorId)
        {
            ActorId = actorId;
        }

        // 角色ID为-1时 (byte值就是255) 表示当前受玩家操作的主角
        public int ActorId { get; }
    }
}