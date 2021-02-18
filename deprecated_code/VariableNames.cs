using System.Collections.Generic;
using System;

namespace Luajit_Decompiler.dec.data
{
    class VariableNames
    {
        private Dictionary<string, bool> accessedVarNames;
        private List<string> varNames;
        private int generatedVarNameIndex = 0;

        public VariableNames(List<string> varNames)
        {
            this.varNames = varNames;
            accessedVarNames = new Dictionary<string, bool>();
        }

        public (string, string) GetVarNameAndIfLocal(int slotIndex)
        {
            string varname = GetVariableName(slotIndex);
            string local = "";
            if (HasBeenAccessed(varname))
                local = "local ";
            return (varname, local);
        }

        public (string, bool) GetVarNameAndCheckAccessed(int slotIndex)
        {
            string varName = GetVariableName(slotIndex);
            try
            {
                return (varName, accessedVarNames[varName]);
            }
            catch(KeyNotFoundException e)
            {
                return (varName, false);
            }
        }

        public string GetVarNameAndSetAccessed(int slotIndex)
        {
            string varName = GetVariableName(slotIndex);
            SetAccessed(varName);
            return varName;
        }

        public bool HasBeenAccessed(string varName)
        {
            return accessedVarNames[varName];
        }

        public void SetAccessed(string varName)
        {
            accessedVarNames.Add(varName, true);
        }

        public string GetVariableName(int slotIndex)
        {
            try
            {
                return varNames[slotIndex];
            }
            catch(ArgumentOutOfRangeException e)
            {
                return GenerateVariableName();
            }
        }

        private string GenerateVariableName()
        {
            string varName = "slot_" + generatedVarNameIndex++;
            varNames.Add(varName);
            return varName;
        }
    }
}
