using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace uConstruct
{
    interface IPlacingModifier
    {
        void RenderEditor();
        void Create();
    }
}