using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aurora.ScriptEngine.AuroraDotNetEngine;
using IScriptAPI = Aurora.ScriptEngine.AuroraDotNetEngine.IScriptApi;
using LSL_Float = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLFloat;
using LSL_Integer = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLInteger;
using LSL_Key = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.key;
using LSL_List = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.list;
using LSL_Rotation = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Quaternion;
using LSL_String = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_Vector = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Vector3;

namespace Aurora.Addon.VirtualTokens
{
    public interface IVirtualTokensScriptAPI
    {
        LSL_Key vtIssueTokenAmount(LSL_Key token, LSL_Key recipient, LSL_Integer amount, LSL_String message);
    }
}
