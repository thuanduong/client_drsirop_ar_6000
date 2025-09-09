using System;
using UnityEngine;

namespace Core.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }
}