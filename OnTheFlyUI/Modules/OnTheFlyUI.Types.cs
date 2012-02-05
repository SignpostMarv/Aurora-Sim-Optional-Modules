using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenMetaverse;
using OpenMetaverse.StructuredData;

using Aurora.Framework;

namespace Aurora.Addon.OnTheFlyUI
{
    
    public class Descriptor : List<IContainer>, IDataTransferableOSDMap
    {
        private List<string> m_Contexts;
        private List<string> m_Messages;
        private List<IContainer> m_Containers;

        public Descriptor(IEnumerable<string> contexts, IEnumerable<IContainer> containers)
        {
            m_Contexts = new List<string>(contexts);
            m_Messages = new List<string>(0);
            m_Containers = new List<IContainer>(containers);
        }

        public Descriptor(IEnumerable<string> contexts, IEnumerable<string> messages, IEnumerable<IContainer> containers)
        {
            m_Contexts = new List<string>(contexts);
            m_Messages = new List<string>(messages);
            m_Containers = new List<IContainer>(containers);
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

    public class CategoryList<T> : Container<CategoryList<T>>
    {
        public new string Type
        {
            get { return base.Type + ":CategoryList"; }
        }

        private string m_Name;
        public string Name
        {
            get { return m_Name; }
        }

        private Container<T> m_Description;
        public Container<T> Description
        {
            get { return m_Description; }
        }

        public CategoryList(string name, Container<T> description)
            : base()
        {
            m_Name = name;
            m_Description = description;
        }

        public CategoryList(string name, Container<T> description, IEnumerable<CategoryList<T>> children)
            : base(children)
        {
            m_Name = name;
            m_Description = description;
        }

        public CategoryList(string name, Container<T> description, Attributes attributes, IEnumerable<CategoryList<T>> children)
            : base(attributes, children)
        {
            m_Name = name;
            m_Description = description;
        }

        public CategoryList(string name, Container<T> description, Attributes attributes, DataBind databind, IEnumerable<CategoryList<T>> children)
            : base(attributes, databind, children)
        {
            m_Name = name;
            m_Description = description;
        }

        public CategoryList(string name, Container<T> description, Attributes attributes)
            : base(attributes)
        {
        }

        public CategoryList(Attributes attributes, DataBind databind, Container<T> description)
            : base(attributes, databind)
        {
            m_Name = "foo";
            m_Description = description;
        }

        public new OSD ToOSD()
        {
            OSDMap resp = new OSDMap();
            resp["Type"] = new OSDString(Type);
            resp["Name"] = new OSDString(Name);
            resp["Description"] = Description.ToOSD();
            resp["Attributes"] = Attributes.ToOSD();
            resp["DataBind"] = DataBind.ToOSD();

            OSDArray children = new OSDArray(this.Count);
            if (children.Count > 0)
            {
                List<CategoryList<T>> subCategories = this.OrderBy(x => x.Name).ToList<CategoryList<T>>();
                foreach (CategoryList<T> child in subCategories)
                {
                    children.Add(child.ToOSD());
                }
            }
            resp["Children"] = children;

            return resp;
        }
    }

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

        public String(DataBind databind)
            : base(databind)
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

        public Integer(DataBind databind)
            : base(databind)
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
            m_DataBind = new DataBind(0);
        }

        public Image(UUID value)
            : base(value)
        {
            m_DataBind = new DataBind(0);
        }

        public Image(DataBind databind)
        {
            m_DataBind = databind;
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

        public Icon(DataBind databind)
            : base(databind)
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

        public TableRow()
            : base()
        {
        }

        public TableRow(IEnumerable<ITableRowChild> children)
            : base(children)
        {
        }

        public TableRow(Attributes attributes, IEnumerable<ITableRowChild> children)
            : base(attributes, children)
        {
        }

        public TableRow(Attributes attributes, DataBind databind, IEnumerable<ITableRowChild> children)
            : base(attributes, databind, children)
        {
        }

        public TableRow(Attributes attributes)
            : base(attributes)
        {
        }

        public TableRow(Attributes attributes, DataBind databind)
            : base(attributes, databind)
        {
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
}
