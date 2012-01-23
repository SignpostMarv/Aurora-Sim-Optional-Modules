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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nini.Config;

using OpenMetaverse;

using OpenSim.Services.Interfaces;
using OpenSim.Region.Framework.Scenes;

using Aurora.Framework;
using Aurora.Simulation.Base;

using OpenMetaverse.StructuredData;

namespace Aurora.Addon.VirtualTokens
{
    
    #region Types

    public class VirtualToken : Query2Type<VirtualToken>
    {
        public UUID id = UUID.Zero;
        public string code = "";
        public uint estate = 1;
        public UUID founder = UUID.Zero;
        public DateTime created;
        public UUID icon = UUID.Zero;
        public bool overridable = false;
        public UUID category = UUID.Zero;
        public string name = "";
        public string description = "";
        public bool enabled = true;

        #region Query2Type members

        public new static List<VirtualToken> doQuery2Type(List<string> query)
        {
            List<VirtualToken> tokens = new List<VirtualToken>(query.Count % 11 == 0 ? query.Count / 11 : 0);

            if (query.Count % 11 == 0)
            {
                for (int i = 0; i < query.Count; i += 11)
                {
                    uint foo;
                    bool overridable = uint.TryParse(query[i + 6], out foo) ? foo >= 1 : bool.Parse(query[i + 6]);
                    bool enabled = uint.TryParse(query[i + 8], out foo) ? foo >= 1 : bool.Parse(query[i + 8]);

                    tokens.Add(new VirtualToken
                    {
                        id = UUID.Parse(query[i]),
                        code = query[i + 1],
                        estate = uint.Parse(query[i + 2]),
                        founder = UUID.Parse(query[i + 3]),
                        created = Utils.UnixTimeToDateTime(int.Parse(query[i + 4])),
                        icon = UUID.Parse(query[i + 5]),
                        overridable = overridable,
                        category = UUID.Parse(query[i + 7]),
                        enabled = enabled,
                        name = query[i + 9].Trim(),
                        description = query[i + 10].Trim()
                    });
                }
            }

            return tokens;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(11);

            resp["id"] = id;
            resp["code"] = code;
            resp["estate"] = estate;
            resp["founder"] = founder;
            resp["created"] = Utils.DateTimeToUnixTime(created);
            resp["icon"] = icon;
            resp["overridable"] = overridable;
            resp["category"] = category;
            resp["name"] = name;
            resp["description"] = description;
            resp["enabled"] = enabled;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("id"))
            {
                id = UUID.Parse(map["id"].ToString());
            }
            if (map.ContainsKey("code"))
            {
                code = map["code"].ToString().Trim();
            }
            if (map.ContainsKey("estate"))
            {
                estate = uint.Parse(map["estate"].ToString());
            }
            if (map.ContainsKey("founder"))
            {
                founder = UUID.Parse(map["founder"].ToString());
            }
            if (map.ContainsKey("created"))
            {
                created = Utils.UnixTimeToDateTime(int.Parse(map["created"].ToString()));
            }
            if (map.ContainsKey("icon"))
            {
                icon = UUID.Parse(map["icon"].ToString());
            }
            if (map.ContainsKey("overridable"))
            {
                overridable = bool.Parse(map["overridable"].ToString());
            }
            if (map.ContainsKey("category"))
            {
                category = UUID.Parse(map["category"].ToString());
            }
            if (map.ContainsKey("name"))
            {
                name = map["name"].ToString().Trim();
            }
            if (map.ContainsKey("description"))
            {
                description = map["description"].ToString().Trim();
            }
            if (map.ContainsKey("enabled"))
            {
                enabled = bool.Parse(map["enabled"].ToString());
            }
        }

        public override bool Equals(object obj)
        {
            VirtualToken tok = obj as VirtualToken;
            return (
                tok.id == id &&
                tok.code == code &&
                tok.estate == estate &&
                tok.founder == founder &&
                tok.created.CompareTo(created) == 0 &&
                tok.icon == icon &&
                tok.overridable == overridable &&
                tok.category == category &&
                tok.name == name &&
                tok.description == description &&
                tok.enabled == enabled
            );
        }

        #endregion

        #endregion
    }

    public class VirtualTokenIssuer : Query2Type<VirtualTokenIssuer>
    {
        public UUID tokenID = UUID.Zero;
        public UUID userID = UUID.Zero;
        public bool canIssueChildTokens = false;
        public bool enabled;

        #region Query2Type members

        public new static List<VirtualTokenIssuer> doQuery2Type(List<string> query)
        {
            List<VirtualTokenIssuer> issuers = new List<VirtualTokenIssuer>(query.Count % 4 == 0 ? query.Count / 4 : 0);

            if (query.Count % 4 == 0)
            {
                for (int i = 0; i < query.Count; i += 4)
                {
                    uint foo;
                    bool canIssueChildTokens = uint.TryParse(query[i + 2], out foo) ? foo >= 1 : bool.Parse(query[i + 2]);
                    bool enabled = uint.TryParse(query[i + 3], out foo) ? foo >= 1 : bool.Parse(query[i + 3]);
                    issuers.Add(new VirtualTokenIssuer
                    {
                        tokenID = UUID.Parse(query[i]),
                        userID = UUID.Parse(query[i + 1]),
                        canIssueChildTokens = canIssueChildTokens,
                        enabled = enabled
                    });
                }
            }

            return issuers;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(4);

            resp["tokenID"] = tokenID;
            resp["userID"] = userID;
            resp["canIssueChildTokens"] = canIssueChildTokens;
            resp["enabled"] = enabled;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("tokenID"))
            {
                tokenID = UUID.Parse(map["tokenID"].ToString());
            }
            if (map.ContainsKey("userID"))
            {
                userID = UUID.Parse(map["userID"].ToString());
            }
            if (map.ContainsKey("canIssueChildTokens"))
            {
                canIssueChildTokens = bool.Parse(map["canIssueChildTokens"].ToString());
            }
            if (map.ContainsKey("enabled"))
            {
                enabled = bool.Parse(map["enabled"].ToString());
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenIssuer isur = obj as VirtualTokenIssuer;
            return(
                isur.tokenID == tokenID &&
                isur.userID == userID &&
                isur.canIssueChildTokens == canIssueChildTokens &&
                isur.enabled == enabled
            );
        }

        #endregion

        #endregion
    }

    public class VirtualTokenCategory : Query2Type<VirtualTokenCategory>
    {
        public UUID id = UUID.Zero;
        public UUID parent = UUID.Zero;
        public string name = string.Empty;
        public string description = string.Empty;
        public DateTime created;

        #region Query2Type members

        public new static List<VirtualTokenCategory> doQuery2Type(List<string> query)
        {
            List<VirtualTokenCategory> categories = new List<VirtualTokenCategory>(query.Count % 5 == 0 ? query.Count / 5 : 0);

            if (query.Count % 5 == 0)
            {
                for (int i = 0; i < query.Count; i += 5)
                {
                    categories.Add(new VirtualTokenCategory
                    {
                        id = UUID.Parse(query[i]),
                        parent = UUID.Parse(query[i + 1]),
                        name = query[i + 2].Trim(),
                        description = query[i + 3].Trim(),
                        created = Utils.UnixTimeToDateTime(int.Parse(query[i + 4]))
                    });
                }
            }

            return categories;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap();

            resp["id"] = id;
            resp["parent"] = parent;
            resp["name"] = name;
            resp["description"] = description;
            resp["created"] = Utils.DateTimeToUnixTime(created);

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("id"))
            {
                id = UUID.Parse(map["id"].ToString());
            }
            if (map.ContainsKey("parent"))
            {
                parent = UUID.Parse(map["parent"].ToString());
            }
            if (map.ContainsKey("name"))
            {
                name = map["name"].ToString();
            }
            if (map.ContainsKey("description"))
            {
                description = map["description"].ToString();
            }
            if (map.ContainsKey("created"))
            {
                created = Utils.UnixTimeToDateTime(int.Parse(map["created"].ToString()));
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenCategory cat = obj as VirtualTokenCategory;
            return (
                cat.id == id &&
                cat.parent == parent &&
                cat.name == name &&
                cat.description == description &&
                cat.created.CompareTo(created) == 0
            );
        }
   
        #endregion
    
        #endregion
    }

    public enum VirtualTokenTransactionType{
        Unknown = -1,
        System
    }

    public class VirtualTokenTransaction : Query2Type<VirtualTokenTransaction>{
        public UUID id = UUID.Zero;
        public UUID tokenID = UUID.Zero;
        public UUID sender = UUID.Zero;
        public UUID recipient = UUID.Zero;
        public DateTime issuedOn;
        public VirtualTokenTransactionType type;
        public int amount=0;
        public bool verified=false;
        private string m_message = string.Empty;
        public string message{
            get{
                return m_message;
            }
            set{
                m_message = value.Trim();
            }
        }

        #region Query2Type members
        
        public new static List<VirtualTokenTransaction> doQuery2Type(List<string> query){
            List<VirtualTokenTransaction> transactions = new List<VirtualTokenTransaction>(query.Count % 9 == 0 ? query.Count / 9 : 0);

            if(query.Count % 9 == 0){
                for(int i=0;i<query.Count; i += 9){
                    uint type = uint.Parse(query[i + 5]);
                    uint foo;
                    bool verified = uint.TryParse(query[i + 7], out foo) ? foo >= 1 : bool.Parse(query[i + 7]);
                    transactions.Add(new VirtualTokenTransaction{
                        id = UUID.Parse(query[i]),
                        tokenID = UUID.Parse(query[i + 1]),
                        sender = UUID.Parse(query[i + 2]),
                        recipient = UUID.Parse(query[i + 3]),
                        issuedOn = Utils.UnixTimeToDateTime(int.Parse(query[i + 4])),
                        type = (type <= (uint)Enum.GetValues(typeof(VirtualTokenTransactionType)).Cast<VirtualTokenTransactionType>().Max()) ? (VirtualTokenTransactionType)type : VirtualTokenTransactionType.Unknown,
                        amount = int.Parse(query[i + 6]),
                        verified = verified,
                        message = query[i + 8]
                    });
                }
            }

            return transactions;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(9);

            resp["id"] = id;
            resp["tokenID"] = tokenID;
            resp["sender"] = sender;
            resp["recipient"] = recipient;
            resp["issuedOn"] = Utils.DateTimeToUnixTime(issuedOn);
            resp["type"] = (int)type;
            resp["amount"] = amount;
            resp["verified"] = verified;
            resp["message"] = message;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if(map.ContainsKey("id")){
                id = UUID.Parse(map["id"].ToString());
            }
            if(map.ContainsKey("tokenID")){
                tokenID = UUID.Parse(map["tokenID"].ToString());
            }
            if(map.ContainsKey("sender")){
                sender = UUID.Parse(map["sender"].ToString());
            }
            if(map.ContainsKey("recipient")){
                recipient = UUID.Parse(map["recipient"].ToString());
            }
            if(map.ContainsKey("issuedOn")){
                issuedOn = Utils.UnixTimeToDateTime(int.Parse(map["issuedOn"].ToString()));
            }
            if(map.ContainsKey("type")){
                uint maptype = uint.Parse(map["type"].ToString());
                type = (maptype <= (uint)Enum.GetValues(typeof(VirtualTokenTransactionType)).Cast<VirtualTokenTransactionType>().Max()) ? (VirtualTokenTransactionType)maptype : VirtualTokenTransactionType.Unknown;
            }
            if(map.ContainsKey("amount")){
                amount = int.Parse(map["amount"].ToString());
            }
            if(map.ContainsKey("verified")){
                verified = bool.Parse(map["verified"].ToString());
            }
            if(map.ContainsKey("message")){
                message = map["message"].ToString();
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenTransaction foo = obj as VirtualTokenTransaction;

            return(
                foo.id == id &&
                foo.tokenID == tokenID &&
                foo.sender == sender &&
                foo.recipient == recipient &&
                foo.issuedOn.CompareTo(issuedOn) == 0 &&
                foo.type == type &&
                foo.amount == amount &&
                foo.verified == verified &&
                foo.message == message
            );
        }

        #endregion

        #endregion
    }

    #endregion

    public class VirtualTokensData : IAuroraDataPlugin
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
                return "VirtualTokensData";
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
                StorageProvider = dbConfig.GetString("StorageProvider", String.Empty);
                ConnectionString = dbConfig.GetString("ConnectionString", String.Empty);
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
            token.id = token.id == UUID.Zero ? UUID.Random() : token.id;
            token.created = DateTime.Now;

            return GD.Insert("as_virtualtokens", new string[11]{
                "id",
                "code",
                "estate",
                "founder",
                "created",
                "icon",
                "overridable",
                "category",
                "name",
                "description",
                "enabled",
            }, new object[11]{
                token.id,
                token.code,
                token.estate,
                token.founder,
                Utils.DateTimeToUnixTime(token.created),
                token.icon,
                token.overridable,
                token.category,
                token.name,
                token.description,
                token.enabled ? 1 : 0
            }) ? token : null;
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

                if (GD.Insert("as_virtualtokens_transactions", new string[9]{
                    "id",
                    "currency",
                    "sender",
                    "recipient",
                    "issuedOn",
                    "type",
                    "amount",
                    "verified",
                    "message"
                }, new object[9]{
                    temp.id,
                    temp.tokenID,
                    temp.sender,
                    temp.recipient,
                    Utils.DateTimeToUnixTime(temp.issuedOn),
                    (uint)temp.type,
                    temp.amount,
                    temp.verified ? 1 : 0,
                    temp.message
                }))
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
            return (GetBalance(token, user) == balance) || GD.Replace("as_virtualtokens_balances", new string[3]{
                "currency",
                "user",
                "balance"
            }, new object[3]{
                token.id,
                user,
                balance
            });
        }

        #endregion

        #region Category

        public VirtualTokenCategory AddCategory(VirtualTokenCategory category)
        {
            category.id = category.id == UUID.Zero ? UUID.Random() : category.id;
            category.created = DateTime.Now;

            return GD.Insert("as_virtualtokens_category", new string[5]{
                "id",
                "parent",
                "name",
                "description",
                "created"
            }, new object[5]{
                category.id,
                category.parent,
                category.name,
                category.description,
                Utils.DateTimeToUnixTime(category.created)
            }) ? category : null;
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
            return GD.Insert("as_virtualtokens_issuers", new string[4]{
               "currency",
               "issuer",
               "issueChildTokens",
               "enabled"
            }, new object[4]{
                issuer.tokenID,
                issuer.userID,
                issuer.canIssueChildTokens ? 1 : 0,
                issuer.enabled ? 1 : 0
            });
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
    }

    public class VirtualTokensService : IService
    {
        private string defaultIssuerPassword;
        private string defaultIssuerName;
        private UUID defaultIssuerUUID;
        UserAccount defaultIssuer;

        bool m_enabled = false;
        IRegistryCore m_registry;

        VirtualTokensData m_vtd;

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
                m_vtd = DataManager.DataManager.RequestPlugin<VirtualTokensData>();
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
                    userservice.CreateUser(defaultIssuerUUID, defaultIssuerName, defaultIssuerPassword, "");
                }
                else
                {
                    userservice.CreateUser(defaultIssuerName, defaultIssuerPassword, "");
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
}
