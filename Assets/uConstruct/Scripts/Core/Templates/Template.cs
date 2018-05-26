using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace uConstruct.Core.Templates
{
    public class Template : MonoBehaviour
    {
        [HideInInspector]
        public ITemplateObject[] templateObjects;

        [HideInInspector]
        public string templateName;
    }
}