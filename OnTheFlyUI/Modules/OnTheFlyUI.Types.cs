using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenMetaverse;
using OpenMetaverse.StructuredData;

using Aurora.Framework;

namespace Aurora.Addon.OnTheFlyUI
{
    #region Generic elements

    public class String : Element<string>
    {
        public new string Type
        {
            get { return base.Type + ":String"; }
        }

        public String()
            : base()
        {
        }

        public String(string value)
            : base(value)
        {
        }
    }

    public class Integer : Element<int>
    {
        public new string Type
        {
            get { return base.Type + ":Integer"; }
        }

        public Integer()
            : base()
        {
        }

        public Integer(int value)
            : base(value)
        {
        }
    }

    #endregion

    #region Image elements

    public class Image : Element<UUID>
    {
        public new string Type
        {
            get { return base.Type + ":Image"; }
        }

        public Image()
            : base()
        {
        }

        public Image(UUID value)
            : base(value)
        {
        }
    }

    public class Icon : Image
    {
        public new string Type
        {
            get { return base.Type + ":Icon"; }
        }

        public Icon()
            : base()
        {
        }

        public Icon(UUID value)
            : base(value)
        {
        }
    }

    #endregion

    #region Table elements

    public class Table : Container<ITableChild>
    {
        public new string Type
        {
            get { return base.Type + ":Table"; }
        }

        public Table()
            : base()
        {
        }

        public Table(IEnumerable<ITableChild> children)
            : base(children)
        {
        }

        public Table(Attributes attributes, IEnumerable<ITableChild> children)
            : base(attributes, children)
        {
        }

        public Table(Attributes attributes, DataBind databind, IEnumerable<ITableChild> children)
            : base(attributes, databind, children)
        {
        }

        public Table(Attributes attributes)
            : base(attributes)
        {
        }

        public Table(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
        }
    }

    public class TableRow : ElementParent<ITableRowChild>, ITableChild
    {
        public new string Type
        {
            get { return base.Type + ":TableRow"; }
        }
    }

    public class TableHeaderCell : ElementParent<IElement>, ITableRowChild
    {
        public new string Type
        {
            get { return base.Type + ":TableHeaderCell"; }
        }

        public TableHeaderCell()
            : base()
        {
        }

        public TableHeaderCell(IEnumerable<IElement> children)
            : base(children)
        {
        }

        public TableHeaderCell(Attributes attributes, IEnumerable<IElement> children)
            : base(attributes, children)
        {
        }

        public TableHeaderCell(Attributes attributes, DataBind databind, IEnumerable<IElement> children)
            : base(attributes, databind, children)
        {
        }

        public TableHeaderCell(Attributes attributes)
            : base(attributes)
        {
        }

        public TableHeaderCell(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
        }
    }

    public class TableDataCell : ElementParent<IElement>, ITableRowChild
    {
        public new string Type
        {
            get { return base.Type + ":TableHeaderCell"; }
        }

        public TableDataCell()
            : base()
        {
        }

        public TableDataCell(IEnumerable<IElement> children)
            : base(children)
        {
        }

        public TableDataCell(Attributes attributes, IEnumerable<IElement> children)
            : base(attributes, children)
        {
        }

        public TableDataCell(Attributes attributes, DataBind databind, IEnumerable<IElement> children)
            : base(attributes, databind, children)
        {
        }

        public TableDataCell(Attributes attributes)
            : base(attributes)
        {
        }

        public TableDataCell(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
        }
    }

    #endregion
    
    public class Descriptor : List<IContainer>, IDataTransferableOSDMap
    {
        private List<string> m_Contexts;
        private List<string> m_Messages;
        private List<IContainer> m_Containers;

        public Descriptor(List<string> contexts, List<IContainer> containers)
        {
            m_Contexts = contexts;
            m_Messages = new List<string>(0);
            m_Containers = containers;
        }

        public Descriptor(List<string> contexts, List<string> messages, List<IContainer> containers)
        {
            m_Contexts = contexts;
            m_Messages = messages;
            m_Containers = containers;
        }

        public OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap();
            resp["Contexts"] = new OSDArray(m_Contexts.ConvertAll<OSD>(x => new OSDString(x)));
            resp["Messages"] = new OSDArray(m_Messages.ConvertAll<OSD>(x => new OSDString(x)));
            resp["Containers"] = new OSDArray(m_Containers.ConvertAll(x => x.ToOSD()));

            return resp;
        }
    }
}
