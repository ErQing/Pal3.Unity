﻿// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2022, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Scene.SceneObjects
{
    using Core.DataReader.Scn;

    [ScnSceneObject(ScnSceneObjectType.SavingPoint)]
    [ScnSceneObject(ScnSceneObjectType.WishPool)]
    [ScnSceneObject(ScnSceneObjectType.SpecialMechanism)]
    public class DisabledSceneObject : SceneObject
    {
        public DisabledSceneObject(ScnObjectInfo objectInfo, ScnSceneInfo sceneInfo)
            : base(objectInfo, sceneInfo, hasModel: false)
        {
        }
    }
}