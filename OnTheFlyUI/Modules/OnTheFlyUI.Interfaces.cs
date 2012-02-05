using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenMetaverse;
using OpenMetaverse.StructuredData;

using Aurora.Framework;

namespace Aurora.Addon.OnTheFlyUI
{
    public interface IOnTheFlyUI
    {
        Descriptor OnTheFlyUI();
    }
    public class Attributes : Dictionary<string, object>, IDataTransferableOSDMap
    {
        public OSDMap ToOSD()
        {
            return Util.DictionaryToOSD(this);
        }

        public Attributes(int capacity)
            : base(capacity)
        {
        }
    }

    public class DataBind : Dictionary<string, string>, IDataTransferableOSDMap
    {
        public OSDMap ToOSD()
        {
            OSDMap map = new OSDMap();
            foreach (KeyValuePair<string, string> kvp in this)
            {
                map[kvp.Key] = new OSDString(kvp.Value);
            }
            return map;
        }

        public DataBind(int capacity)
            : base(capacity)
        {
        }
    }

    public interface IContainer : IDataTransferableOSD
    {
        string Type { get; }
        Attributes Attributes { get; }
        DataBind DataBind { get; }
    }

    public interface IElement : IDataTransferableOSD
    {
        string Type { get; }
        Attributes Attributes { get; }
        DataBind DataBind { get; }
    }

    #region Table-like content

    public interface ITableChild : IContainer
    {
    }

    public interface ITableRowChild
    {
    }

    #endregion

    #region Categories

    public interface Category
    {
        string Name { get; }
        List<Category> Categories { get; }
    }

    #endregion
}
