/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Nini.Config;

using Nwc.XmlRpc;

using OpenMetaverse;

using OpenSim.Services.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;

using Aurora.Framework;
using Aurora.Simulation.Base;

using OpenMetaverse.StructuredData;

using Aurora.ScriptEngine.AuroraDotNetEngine;
using IScriptAPI = Aurora.ScriptEngine.AuroraDotNetEngine.IScriptApi;
using LSL_Float = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLFloat;
using LSL_Integer = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLInteger;
using LSL_Key = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.key;
using LSL_List = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.list;
using LSL_Rotation = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Quaternion;
using LSL_String = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.LSLString;
using LSL_Vector = Aurora.ScriptEngine.AuroraDotNetEngine.LSL_Types.Vector3;

using Aurora.Addon.OnTheFlyUI;

namespace Aurora.Addon.VirtualTokens
{
    public class VirtualTokensConnector : IAuroraDataPlugin
    {
        private bool m_enabled = false;
        public bool Enabled
        {
            get { return m_enabled; }
        }

        private IGenericData GD;
        private string ConnectionString = "";
        private string StorageProvider = "";

        #region console wrappers

        private void Info(object message)
        {
            MainConsole.Instance.Info("[" + Name + "]: " + message.ToString());
        }

        private void Warn(object message)
        {
            MainConsole.Instance.Warn("[" + Name + "]: " + message.ToString());
        }

        #endregion

        #region IAuroraDataPlugin Members

        public string Name
        {
            get
            {
                return "VirtualTokensConnector";
            }
        }

        private void handleConfig(IConfigSource m_config)
        {
            IConfig config = m_config.Configs["Virtual Tokens"];
            if (config == null)
            {
                m_enabled = false;
                Warn("not loaded, no configuration found.");
                return;
            }

            IConfig dbConfig = m_config.Configs["DatabaseService"];
            if (dbConfig != null)
            {
                StorageProvider = dbConfig.GetString("StorageProvider", string.Empty);
                ConnectionString = dbConfig.GetString("ConnectionString", string.Empty);
            }

            StorageProvider = config.GetString("StorageProvider", StorageProvider);
            ConnectionString = config.GetString("ConnectionString", ConnectionString);

            if (StorageProvider == string.Empty || ConnectionString == string.Empty)
            {
                m_enabled = false;
                Warn("not loaded, no storage parameters found");
                return;
            }

            m_enabled = config.GetBoolean("Enabled", false);
        }

        public void Initialize(IGenericData GenericData, IConfigSource source, IRegistryCore simBase, string DefaultConnectionString)
        {
            handleConfig(source);
            if (!Enabled)
            {
                Warn("not loaded, disabled in config.");
                return;
            }
            DataManager.DataManager.RegisterPlugin(this);

            GD = GenericData;
            GD.ConnectToDatabase(ConnectionString, "VirtualTokens", true);
        }

        #endregion

        #region Tokens

        public VirtualToken AddToken(VirtualToken token)
        {
            token.id = (token.id == UUID.Zero && token.estate != 1) ? UUID.Random() : token.id;
            token.created = DateTime.Now;

            Dictionary<string, object> row = new Dictionary<string, object>(11);
            row["id"] = token.id;
            row["code"] = token.code;
            row["estate"] = token.estate;
            row["founder"] = token.founder;
            row["created"] = Utils.DateTimeToUnixTime(token.created);
            row["icon"] = token.icon;
            row["overridable"] = token.overridable;
            row["category"] = token.category;
            row["name"] = token.name;
            row["description"] = token.description;
            row["enabled"] = token.enabled ? 1 : 0;

            return GD.Insert("as_virtualtokens", row) ? token : null;
        }

        public bool UpdateToken(VirtualToken token)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(8);
            values["code"] = token.code;
            values["estate"] = token.estate;
            values["icon"] = token.icon;
            values["overridable"] = token.overridable;
            values["category"] = token.category;
            values["enabled"] = token.enabled;
            values["name"] = token.name;
            values["description"] = token.description;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = token.id;

            return GD.Update("as_virtualtokens", values, null, filter, null, null);
        }

        public VirtualToken GetToken(UUID id)
        {
            VirtualToken token = null;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = id;
            List<VirtualToken> query = VirtualToken.doQuery2Type(GD.Query(new string[1] { "*" }, "as_virtualtokens", filter, null, 0, 1));

            if (query.Count != 1)
            {
                Warn("No virtual token could be found with ID " + id);
            }
            else
            {
                token = query[0];
            }

            return token;
        }

        public VirtualToken GetToken(string code, uint estateID)
        {
            VirtualToken token = null;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["code"] = code;
            filter.andFilters["estate"] = estateID;
            List<VirtualToken> query = VirtualToken.doQuery2Type(GD.Query(new string[1] { "*" }, "as_virtualtokens", filter, null, 0, 1));

            if (query.Count != 1)
            {
                Warn("No virtual token could be found with ID " + estateID);
            }
            else
            {
                token = query[0];
            }

            return token;
        }

        public List<VirtualToken> GetToken(string code)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["code"] = code;
            return VirtualToken.doQuery2Type(GD.Query(new string[11] {
                "id",
                "code",
                "estate",
                "founder",
                "created",
                "icon",
                "overridable",
                "category",
                "enabled",
                "name",
                "description"
            }, "as_virtualtokens", filter, null, null, null));
        }

        public List<VirtualToken> GetTokenParents(VirtualToken token, bool stopAtNonOverridable)
        {
            List<VirtualToken> tokens = new List<VirtualToken>();

            IEstateConnector estateConnector = Aurora.DataManager.DataManager.RequestPlugin<IEstateConnector>();

            if (estateConnector != null)
            {
                VirtualToken currentToken = token;
                EstateSettings es;
                while (currentToken.estate > 1)
                {
                    es = estateConnector.GetEstateSettings((int)currentToken.estate);
                    if (es == null)
                    {
                        break;
                    }
                    currentToken = GetToken(currentToken.code, es.ParentEstateID);
                    if (currentToken == null)
                    {
                        break;
                    }
                    tokens.Add(currentToken);
                    if (stopAtNonOverridable && currentToken.overridable == false)
                    {
                        break;
                    }
                }
            }
            else
            {
                Warn("Could not get IEstateConnector");
            }

            return tokens;
        }

        #endregion

        #region Tranactions & Balances

        public VirtualTokenTransaction IssueTokenAmount(VirtualToken token, UUID issuer, UUID recipient, int amount, string message)
        {
            VirtualTokenTransaction transaction = null;

            if (amount == 0)
            {
                Warn("Cannot issue 0 tokens");
            }else if (!isValidIssuer(token, issuer))
            {
                Warn(string.Format("{0} is not a valid issuer for token {1} ({2})", issuer, token.code, token.id));
            }
            else
            {
                VirtualTokenTransaction temp = new VirtualTokenTransaction
                {
                    id = UUID.Random(),
                    tokenID = token.id,
                    sender = issuer,
                    recipient = recipient,
                    issuedOn = DateTime.Now,
                    type = VirtualTokenTransactionType.System,
                    amount = amount,
                    message = message
                };

                Dictionary<string, object> row = new Dictionary<string, object>(9);
                row["id"] = temp.id;
                row["currency"] = temp.tokenID;
                row["sender"] = temp.sender;
                row["recipient"] = temp.recipient;
                row["issuedOn"] = Utils.DateTimeToUnixTime(temp.issuedOn);
                row["type"] = (uint)temp.type;
                row["amount"] = temp.amount;
                row["verified"] = temp.verified ? 1 : 0;
                row["message"] = temp.message;

                if (GD.Insert("as_virtualtokens_transactions", row))
                {
                    if (SetBalance(token, recipient, GetBalance(token, recipient) + amount))
                    {
                        transaction = temp;
                    }
                    else
                    {
                        Warn("Transaction was made, but could not update balance. Transaction ID: " + temp.id);
                    }
                }
                else
                {
                    Warn("Transaction was not recorded, balance will not be updated.");
                }
            }

            return transaction;
        }

        public int GetBalance(VirtualToken token, UUID user)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["currency"] = token.id;
            filter.andFilters["user"] = user;
            List<string> query = GD.Query(new string[1] { "balance" }, "as_virtualtokens_balances", filter, null, 0, 1);

            return query.Count == 1 ? int.Parse(query[0]) : 0;
        }

        private bool SetBalance(VirtualToken token, UUID user, int balance)
        {
            Dictionary<string, object> row = new Dictionary<string, object>(3);
            row["currency"] = token.id;
            row["user"] = user;
            row["balance"] = balance;
            return (GetBalance(token, user) == balance) || GD.Replace("as_virtualtokens_balances", row);
        }

        #endregion

        #region Category

        public VirtualTokenCategory AddCategory(VirtualTokenCategory category)
        {
            category.id = category.id == UUID.Zero ? UUID.Random() : category.id;
            category.created = DateTime.Now;

            Dictionary<string, object> row = new Dictionary<string, object>(5);
            row["id"] = category.id;
            row["parent"] = category.parent;
            row["name"] = category.name;
            row["description"] = category.description;
            row["created"] = Utils.DateTimeToUnixTime(category.created);

            return GD.Insert("as_virtualtokens_category", row) ? category : null;
        }

        public bool UpdateCategory(VirtualTokenCategory category)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(3);
            values["parent"] = category.parent;
            values["name"] = category.name;
            values["description"] = category.description;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = category.id;

            return GD.Update("as_virtualtokens_category", values, null, filter, null, null);
        }

        public VirtualTokenCategory GetCategory(UUID id)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = id;
            List<VirtualTokenCategory> categories = VirtualTokenCategory.doQuery2Type(GD.Query(new string[5]{
                "id",
                "parent",
                "name",
                "description",
                "created"
            }, "as_virtualtokens_category", filter, null, 1, null));

            return categories.Count == 1 ? categories[0] : null;
        }

        public VirtualTokenCategory GetCategory(string name)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["name"] = name.Trim();
            List<VirtualTokenCategory> categories = VirtualTokenCategory.doQuery2Type(GD.Query(new string[5]{
                "id",
                "parent",
                "name",
                "description",
                "created"
            }, "as_virtualtokens_category", filter, null, 1, null));

            return categories.Count == 1 ? categories[0] : null;
        }

        #endregion

        #region Issuers

        public bool AddIssuer(VirtualTokenIssuer issuer)
        {
            Dictionary<string, object> row = new Dictionary<string, object>(4);
            row["currency"] = issuer.tokenID;
            row["issuer"] = issuer.userID;
            row["issueChildTokens"] = issuer.canIssueChildTokens ? 1 : 0;
            row["enabled"] = issuer.enabled ? 1 : 0;
            return GD.Insert("as_virtualtokens_issuers", row);
        }

        public bool UpdateIssuer(VirtualTokenIssuer issuer)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(2);
            values["issueChildTokens"] = issuer.canIssueChildTokens ? 1 : 0;
            values["enabled"] = issuer.enabled ? 1 : 0;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["currency"] = issuer.tokenID;
            filter.andFilters["issuer"] = issuer.userID;

            return GD.Update("as_virtualtokens_issuers", values, null, filter, null, null);
        }

        public VirtualTokenIssuer GetIssuer(UUID id)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = id;
            List<VirtualTokenIssuer> issuers = VirtualTokenIssuer.doQuery2Type(GD.Query(new string[4]{
                "currency",
                "issuer",
                "issueChildTokens",
                "enabled"
            }, "as_virtualtokens_issuers", filter, null, 1, null));

            return issuers.Count == 1 ? issuers[0] : null;
        }

        public List<VirtualTokenIssuer> GetIssuer(VirtualToken token, bool includeParentTokenIssuers)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["currency"] = token.id;
            filter.andFilters["enabled"] = 1;
            List<VirtualTokenIssuer> issuers = VirtualTokenIssuer.doQuery2Type(GD.Query(new string[4]{
                "currency",
                "issuer",
                "issueChildTokens",
                "enabled"
            }, "as_virtualtokens_issuers", filter, null, null, null));

            if (includeParentTokenIssuers)
            {
                foreach (VirtualToken parentToken in GetTokenParents(token, true))
                {
                    issuers.AddRange(GetIssuer(parentToken, false));
                }
            }

            return issuers;
        }

        public bool isValidIssuer(VirtualToken token, UUID issuer)
        {
            List<VirtualTokenIssuer> issuers = GetIssuer(token, true);

            foreach (VirtualTokenIssuer validIssuer in issuers)
            {
                if (validIssuer.userID == issuer)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region OnTheFlyUI members

        public Descriptor OnTheFlyUI()
        {
            List<IContainer> containers = new List<IContainer>();
            
            Attributes left = new Attributes(1);
            left["textAlign"] = "left";
            Attributes right = new Attributes(1);
            right["textAlign"] = "right";

            DataBind categoryDatabind = new DataBind(2);
            categoryDatabind[":category.Name"] = "Name";
            categoryDatabind[":category.Children[]"] = ":category";

            return new Descriptor(new string[1]{ "user_profile:mine" }, new string[1]{ "VirtualTokensBalanceUpdate" }, new IContainer[]{
                new CategoryList<ITableChild>(left, categoryDatabind,
                    new Table(new Attributes(0), new DataBind(":category.balances", ":balances"), new ITableChild[]{
                        new TableRow(new Attributes(0), new DataBind(":balances[]", ":balance"), new ITableRowChild[]{
                            new TableHeaderCell(left, new IElement[]{
                                new OnTheFlyUI.String(new DataBind(":balances[]token.name", "Value"))
                            }),
                            new TableDataCell(right, new IElement[]{
                                new OnTheFlyUI.Integer(new DataBind(":balance.amount", "Value")),
                                new OnTheFlyUI.Icon(new DataBind(":balance.token.icon", "Value"))
                            })
                        })
                    })
                )
            });
        }

        #endregion
    }

    public class VirtualTokensService : IService
    {
        private string defaultIssuerPassword;
        private string defaultIssuerName;
        private UUID defaultIssuerUUID;
        UserAccount defaultIssuer;

        bool m_enabled = false;
        IRegistryCore m_registry;

        VirtualTokensConnector m_vtd;

        public string Name
        {
            get
            {
                return "VirtualTokensService";
            }
        }

        #region console wrappers

        private void Info(object message)
        {
            MainConsole.Instance.Info("[" + Name + "]: " + message.ToString());
        }

        private void Warn(object message)
        {
            MainConsole.Instance.Warn("[" + Name + "]: " + message.ToString());
        }

        #endregion

        #region IService members

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            m_registry = registry;

            IConfig VTconfig = config.Configs["Virtual Tokens"];
            if (VTconfig != null)
            {
                m_enabled = VTconfig.GetBoolean("Enabled", false);
                if (m_enabled)
                {
                    defaultIssuerName = VTconfig.GetString("DefaultIssuerName", "Token Issuer");
                    defaultIssuerUUID = UUID.Parse(VTconfig.GetString("DefaultIssuerUUID", UUID.Zero.ToString()));
                    string password = VTconfig.GetString("DefaultIssuerPassword", string.Empty).Trim();
                    if (password == string.Empty)
                    {
                        m_enabled = false;
                        Warn("Default Issuer Password was not specified");
                    }
                    else
                    {
                        defaultIssuerPassword = Utils.MD5(password);
                    }
                }
            }

            if (m_enabled)
            {
                registry.RegisterModuleInterface<VirtualTokensService>(this);
            }
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            if (m_enabled)
            {
                m_registry = registry;
                m_vtd = DataManager.DataManager.RequestPlugin<VirtualTokensConnector>();
                if (m_vtd == null)
                {
                    m_enabled = false;
                    Warn("Could not get Virtual Tokens Data Plugin");
                }
                else if (!m_vtd.Enabled)
                {
                    m_enabled = false;
                    Warn("Virtual Tokens Data Plugin was disabled");
                }
            }
            else
            {
                Warn("not loaded");
            }
        }

        public void FinishedStartup()
        {
            IUserAccountService userservice = m_registry.RequestModuleInterface<IUserAccountService>();
            if (m_enabled)
            {
                m_enabled = (userservice != null);
                if (userservice == null)
                {
                    Warn("Could not find user service");
                }
                if (!m_enabled)
                {
                    return;
                }
            }
            else
            {
                return;
            }
            if (defaultIssuerUUID != UUID.Zero)
            {
                Info("Attempting to find default issuer by UUID " + defaultIssuerUUID);
                defaultIssuer = userservice.GetUserAccount(UUID.Zero, defaultIssuerUUID);
            }
            else
            {
                Info("Attempting to find default issuer by name " + defaultIssuerName);
                defaultIssuer = userservice.GetUserAccount(UUID.Zero, defaultIssuerName);
            }
            if (defaultIssuer == null)
            {
                Warn("Default issuer account not found, attempting to create");
                if (defaultIssuerUUID != UUID.Zero)
                {
                    userservice.CreateUser(defaultIssuerUUID, UUID.Zero, defaultIssuerName, "$1$" + defaultIssuerPassword, "");
                }
                else
                {
                    userservice.CreateUser(defaultIssuerName, "$1$" + defaultIssuerPassword, "");
                }
                defaultIssuer = userservice.GetUserAccount(UUID.Zero, defaultIssuerName);
                if (defaultIssuer == null)
                {
                    Warn("Failed to create account for default issuer");
                    m_enabled = false;
                    return;
                }
                else
                {
                    Info(defaultIssuer.Name + " created as default issuer with UUID " + defaultIssuer.PrincipalID);
                }
            }
            else
            {
                Info("Default token issuer named \"" + defaultIssuer.Name + "\" found with UUID " + defaultIssuer.PrincipalID);
            }

            VirtualTokenCategory exampleCategory = m_vtd.GetCategory("Examples");
            if (exampleCategory == null)
            {
                Info("Creating example category.");
                exampleCategory = m_vtd.AddCategory(new VirtualTokenCategory
                {
                    name = "Examples",
                    description = "Category to used for demonstrating VirtualTokens module"
                });
                if (exampleCategory == null)
                {
                    Warn("Could not create example category.");
                    m_enabled = false;
                    return;
                }
            }
            else
            {
                Info("Example category already exists");
            }

            VirtualToken exampleToken = m_vtd.GetToken("XMPL$", 1);
            if (exampleToken == null)
            {
                Info("Creating example token");
                exampleToken = m_vtd.AddToken(new VirtualToken
                {
                    code = "XMPL$",
                    category = exampleCategory.id,
                    name = "Example Dollars",
                    description = "Example Dollar Tokens",
                    founder = defaultIssuer.PrincipalID,
                    overridable = true,
                    enabled = true,
                    estate = 1
                });
                if (exampleToken == null)
                {
                    Warn("Could not create example token.");
                    m_enabled = false;
                    return;
                }
            }

            VirtualTokenIssuer exampleIssuer = new VirtualTokenIssuer
            {
                tokenID = exampleToken.id,
                userID = defaultIssuer.PrincipalID,
                enabled = true,
                canIssueChildTokens = false
            };

            if (m_vtd.isValidIssuer(exampleToken, exampleIssuer.userID))
            {
                Info(string.Format("\"{0}\" is already an issuer for the example token", defaultIssuer.Name));
            }
            else
            {
                Warn(string.Format("\"{0}\" is not an issuer for the example token", defaultIssuer.Name));
                if (!m_vtd.AddIssuer(exampleIssuer))
                {
                    Warn(string.Format("Could not assign \"{0}\" as issuer to example token.", defaultIssuer.Name));
                    m_enabled = false;
                    return;
                }
                else
                {
                    Info(string.Format("\"{0}\" successfully added as an issuer to the example token.", defaultIssuer.Name));
                }
            }

            VirtualTokenTransaction exampleTransaction = m_vtd.IssueTokenAmount(exampleToken, defaultIssuer.PrincipalID, defaultIssuer.PrincipalID, (new System.Random()).Next(int.MinValue, int.MaxValue), "Randomly issued tokens");
            if (exampleTransaction == null)
            {
                Warn("Was not able to give default issuer a random amount of example tokens");
                m_enabled = false;
                return;
            }
            else
            {
                Info(string.Format("Successfully gave default issuer {0} example tokens", exampleTransaction.amount));
            }

        }

        #endregion

        #region Tokens

        #endregion
    }

    [Serializable]
    public class VirtualTokensScripts : MarshalByRefObject, IVirtualTokensScriptAPI, IScriptApi
    {
        private VirtualTokensConnector m_vtc;

        #region IScriptApi Members

        private UUID m_itemID;
        private IScriptModulePlugin m_ScriptEngine;
        private ISceneChildEntity m_host;
        private ScriptProtectionModule m_ScriptProtection;

        /// <summary>
        ///   One-liner of that created by John Sibly @ http://stackoverflow.com/questions/52797/c-how-do-i-get-the-path-of-the-assembly-the-code-is-in
        /// </summary>
        public static string AssemblyFileName
        {
            get
            {
                return Path.GetFileName(Uri.UnescapeDataString((new UriBuilder(Assembly.GetExecutingAssembly().CodeBase)).Path));
            }
        }

        public IScene World
        {
            get { return m_host.ParentEntity.Scene; }
        }

        public void Initialize(IScriptModulePlugin ScriptEngine, ISceneChildEntity host, uint localID, UUID itemID, ScriptProtectionModule module)
        {
            m_itemID = itemID;
            m_ScriptEngine = ScriptEngine;
            m_host = host;
            m_ScriptProtection = module;
            m_vtc = DataManager.DataManager.RequestPlugin<VirtualTokensConnector>();
        }

        public IScriptApi Copy()
        {
            return new VirtualTokensScripts();
        }

        public string Name
        {
            get { return "vt"; }
        }

        public string InterfaceName
        {
            get { return "IVirtualTokensScriptAPI"; }
        }

        /// <summary>
        ///   We have to add a ref here, as this API is NOT inside of the script engine
        ///   So we add the referenced assembly to ourselves
        /// </summary>
        public string[] ReferencedAssemblies
        {
            get
            {
                return new string[1]{ AssemblyFileName };
            }
        }

        /// <summary>
        ///   We use "Aurora.BotManager", and that isn't a default namespace, so we need to add it
        /// </summary>
        public string[] NamespaceAdditions
        {
            get { return new string[1] {"Aurora.Addon.VirtualTokens"}; }
        }

        #endregion

        #region IVirtualTokensScriptAPI Members

        public LSL_Key vtIssueTokenAmount(LSL_Key token, LSL_Key recipient, LSL_Integer amount, LSL_String message)
        {
            if (m_vtc != null && amount > 0)
            {
                if (!m_ScriptProtection.CheckThreatLevel(Aurora.ScriptEngine.AuroraDotNetEngine.ThreatLevel.Moderate, "vtIssueTokenAmount", m_host, Name, m_itemID))
                {
                    return "";
                }
                if (message.ToString().Trim() == string.Empty)
                {
                    throw new Exception("A non-empty message must be specified.");
                }
                VirtualToken vt = m_vtc.GetToken(new UUID(token.ToString()));
                if (vt == null)
                {
                    throw new Exception("No token could be found with key '" + token.ToString() + "'");
                }
                else if (!m_vtc.isValidIssuer(vt, m_host.OwnerID))
                {
                    throw new Exception("You do not have permission to issue " + vt.code);
                }
                UUID rUUID = new UUID(recipient.ToString());
                VirtualTokenTransaction transaction = m_vtc.IssueTokenAmount(vt, m_host.OwnerID, rUUID, amount, message);
                if (transaction != null)
                {
                    IScenePresence rw = World.GetScenePresence(rUUID);
                    if (rw != null)
                    {
                        rw.ControllingClient.SendAlertMessage(string.Format("You have receieved currency: {0}{1} - {2}", vt.code, amount, message));
                    }
                    return new LSL_Key(transaction.id);
                }
            }

            return new LSL_Key(UUID.Zero);
        }

        #endregion
    }

    public class VirtualTokensCurrencyModule : IMoneyModule, ISharedRegionModule
    {
        private bool m_enabled;
        public bool Enabled
        {
            get { return m_enabled; }
        }

        #region IMoneyModule Members

        public int UploadCharge
        {
            get { return 0; }
        }

        public int GroupCreationCharge
        {
            get { return 0; }
        }

        public int ClientPort
        {
            get
            {
                return (int)MainServer.Instance.Port;
            }
        }

        public event ObjectPaid OnObjectPaid;
        public event PostObjectPaid OnPostObjectPaid;

        public bool Transfer(UUID toID, UUID fromID, int amount, string description)
        {
            return true;
        }

        public bool Transfer(UUID toID, UUID fromID, int amount, string description, TransactionType type)
        {
            return true;
        }

        public bool Transfer(UUID toID, UUID fromID, UUID toObjectID, UUID fromObjectID, int amount, string description, TransactionType type)
        {
            if ((type == TransactionType.PayIntoObject) && (OnObjectPaid != null)){
                OnObjectPaid((fromObjectID == UUID.Zero) ? toObjectID : fromObjectID, fromID, amount);
            }
            return true;
        }

        public void Transfer(UUID objectID, UUID agentID, int amount)
        {
        }

        public int Balance(UUID agentID)
        {
            return 0;
        }

        public bool Charge(UUID agentID, int amount, string text)
        {
            return true;
        }

        public bool ObjectGiveMoney(UUID objectID, UUID fromID, UUID toID, int amount)
        {
            return true;
        }

        public void ProcessMoneyTransferRequest(UUID source, UUID destination, int amount,
                                                int transactiontype, string description)
        {
        }

        public List<GroupAccountHistory> GetTransactions(UUID groupID, UUID agentID, int currentInterval, int intervalDays)
        {
            return new List<GroupAccountHistory>();
        }

        public GroupBalance GetGroupBalance(UUID groupID)
        {
            return new GroupBalance() { StartingDate = DateTime.Now.AddDays(-4) };
        }

        public bool Charge(IClientAPI client, int amount)
        {
            return true;
        }

        #endregion

        #region IRegionModuleBase Members

        /// <summary>
        ///   Startup
        /// </summary>
        /// <param name = "scene"></param>
        /// <param name = "config"></param>
        public void Initialise(IConfigSource config)
        {
            IConfig currencyConfig = config.Configs["Currency"];
            if (currencyConfig != null)
            {
                m_enabled = currencyConfig.GetString("Module", "") == Name;
            }
        }

        public void AddRegion(IScene scene)
        {
            if (!m_enabled)
                return;
            // Send ObjectCapacity to Scene..  Which sends it to the SimStatsReporter.
            scene.RegisterModuleInterface<IMoneyModule>(this);

            // XMLRPCHandler = scene;
            // To use the following you need to add:
            // -helperuri <ADDRESS TO HERE OR grid MONEY SERVER>
            // to the command line parameters you use to start up your client
            // This commonly looks like -helperuri http://127.0.0.1:9000/
            MainServer.Instance.AddXmlRPCHandler("getCurrencyQuote", quote_func);
            MainServer.Instance.AddXmlRPCHandler("buyCurrency", buy_func);
            MainServer.Instance.AddXmlRPCHandler("preflightBuyLandPrep", preflightBuyLandPrep_func);
            MainServer.Instance.AddXmlRPCHandler("buyLandPrep", landBuy_func);

            scene.EventManager.OnNewClient += OnNewClient;
            scene.EventManager.OnClosingClient += OnClosingClient;
        }

        #region XMLRPCHandlers

        protected XmlRpcResponse quote_func(XmlRpcRequest request, IPEndPoint ep)
        {
            Hashtable requestData = (Hashtable)request.Params[0];
            UUID agentId = UUID.Zero;
            int amount = 0;
            Hashtable quoteResponse = new Hashtable();
            XmlRpcResponse returnval = new XmlRpcResponse();

            if (requestData.ContainsKey("agentId") && requestData.ContainsKey("currencyBuy"))
            {
                UUID.TryParse((string)requestData["agentId"], out agentId);
                try
                {
                    amount = (Int32)requestData["currencyBuy"];
                }
                catch (InvalidCastException)
                {
                }
                Hashtable currencyResponse = new Hashtable { { "estimatedCost", 0 }, { "currencyBuy", amount } };

                quoteResponse.Add("success", true);
                quoteResponse.Add("currency", currencyResponse);
                quoteResponse.Add("confirm", "asdfad9fj39ma9fj");

                returnval.Value = quoteResponse;
                return returnval;
            }

            quoteResponse.Add("success", false);
            quoteResponse.Add("errorMessage", "Invalid parameters passed to the quote box");
            quoteResponse.Add("errorURI", "http://www.opensimulator.org/wiki");
            returnval.Value = quoteResponse;
            return returnval;
        }

        protected XmlRpcResponse buy_func(XmlRpcRequest request, IPEndPoint ep)
        {
            /*Hashtable requestData = (Hashtable)request.Params[0];
            UUID agentId = UUID.Zero;
            int amount = 0;
            if (requestData.ContainsKey("agentId") && requestData.ContainsKey("currencyBuy"))
            {
                UUID.TryParse((string)requestData["agentId"], out agentId);
                try
                {
                    amount = (Int32)requestData["currencyBuy"];
                }
                catch (InvalidCastException)
                {
                }
                if (agentId != UUID.Zero)
                {
                    uint buyer = CheckExistAndRefreshFunds(agentId);
                    buyer += (uint)amount;
                    UpdateBalance(agentId,buyer);
					
                    IClientAPI client = LocateClientObject(agentId);
                    if (client != null)
                    {
                        SendMoneyBalance(client, agentId, client.SessionId, UUID.Zero);
                    }
                }
            }*/
            XmlRpcResponse returnval = new XmlRpcResponse();
            Hashtable returnresp = new Hashtable { { "success", true } };
            returnval.Value = returnresp;
            return returnval;
        }

        protected XmlRpcResponse preflightBuyLandPrep_func(XmlRpcRequest request, IPEndPoint ep)
        {
            XmlRpcResponse ret = new XmlRpcResponse();
            Hashtable retparam = new Hashtable();

            Hashtable membershiplevels = new Hashtable();
            membershiplevels.Add("levels", membershiplevels);

            Hashtable landuse = new Hashtable();

            Hashtable level = new Hashtable { { "id", "00000000-0000-0000-0000-000000000000" }, { "", "Premium Membership" } };

            Hashtable currencytable = new Hashtable { { "estimatedCost", 0 } };

            retparam.Add("success", true);
            retparam.Add("currency", currencytable);
            retparam.Add("membership", level);
            retparam.Add("landuse", landuse);
            retparam.Add("confirm", "asdfajsdkfjasdkfjalsdfjasdf");
            ret.Value = retparam;
            return ret;
        }

        protected XmlRpcResponse landBuy_func(XmlRpcRequest request, IPEndPoint ep)
        {
            XmlRpcResponse ret = new XmlRpcResponse();
            Hashtable retparam = new Hashtable { { "success", true } };
            ret.Value = retparam;
            return ret;
        }

        #endregion

        public void RemoveRegion(IScene scene)
        {
        }

        public void RegionLoaded(IScene scene)
        {
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void PostInitialise()
        {
        }

        public void Close()
        {
        }

        public string Name
        {
            get { return "BaseCurrency"; }
        }

        #region Event Handlers

        /// <summary>
        ///   New Client Event Handler
        /// </summary>
        /// <param name = "client"></param>
        protected void OnNewClient(IClientAPI client)
        {
            // Subscribe to Money messages
            client.OnEconomyDataRequest += EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest += SendMoneyBalance;
            client.OnMoneyTransferRequest += ProcessMoneyTransferRequest;
        }

        protected void OnClosingClient(IClientAPI client)
        {
            // Subscribe to Money messages
            client.OnEconomyDataRequest -= EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest -= SendMoneyBalance;
            client.OnMoneyTransferRequest -= ProcessMoneyTransferRequest;
        }

        /// <summary>
        ///   Sends the the stored money balance to the client
        /// </summary>
        /// <param name = "client"></param>
        /// <param name = "agentID"></param>
        /// <param name = "SessionID"></param>
        /// <param name = "TransactionID"></param>
        protected void SendMoneyBalance(IClientAPI client, UUID agentID, UUID SessionID, UUID TransactionID)
        {
            client.SendMoneyBalance(TransactionID, true, new byte[0], 0);
        }

        /// <summary>
        ///   Event called Economy Data Request handler.
        /// </summary>
        /// <param name = "agentId"></param>
        public void EconomyDataRequestHandler(IClientAPI remoteClient)
        {
            remoteClient.SendEconomyData(0, remoteClient.Scene.RegionInfo.ObjectCapacity, 0, 0, 0,
                                         0, 0, 0, 0, 0,
                                         0, 0, 0, 0, 0,
                                         0, 0);
        }

        #endregion

        #endregion
    }
}
