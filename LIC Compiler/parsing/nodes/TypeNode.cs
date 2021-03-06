﻿using System;
using System.Collections.Generic;

namespace LIC.Parsing.Nodes
{
    public class TypeNode : Node
    {
        /// <summary>
        /// Creates new type node on access. Type represents one that is calculated by compiler.
        /// </summary>
        public static TypeNode AutoType => new TypeNode {TypePath = "~auto"};

        /// <summary>
        /// Path separated by colons(:)
        /// </summary>
        public string TypePath { get; set; }

        /// <summary>
        /// Indicates whether type is constant/variable
        /// </summary>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Indicates whether type is dynamic/static
        /// </summary>
        public bool IsDynamic { get; set; }


        /// <summary>
        /// Indicates whether type is reference/value
        /// </summary>
        public bool IsReference { get; set; }

        /// <summary>
        /// Indicates whether type is value/reference
        /// </summary>
        public bool IsValueType { get; set; }


        /// <summary>
        /// Indicates whether type is array/value
        /// </summary>
        public bool IsArrayType { get; set; }

        /// <summary>
        /// Points to type on wich it is referencing
        /// </summary>
        public TypeNode ReferenceType { get; set; }
        
        /// <summary>
        /// Generates string representation of type.
        /// Used for debug/logging purposes.
        /// </summary>
        /// <returns>Formatted string</returns>
        public override string ToString()
        {
            var modifiers = new List<string>(5);
            if (IsConstant)  { modifiers.Add("const"); }
            if (IsDynamic)   { modifiers.Add("dynamic"); }
            if (IsReference) { modifiers.Add("ref"); }
            if (IsValueType) { modifiers.Add("val"); }
            if (IsArrayType) { modifiers.Add("array"); }
            
            var modStr = String.Join(" ", modifiers);
            if (modStr != "") { modStr += " "; }
            
            return ReferenceType == null
                    ? $"{modStr}{TypePath}"
                    : $"{modStr}({ReferenceType})";
        }
    }
}
