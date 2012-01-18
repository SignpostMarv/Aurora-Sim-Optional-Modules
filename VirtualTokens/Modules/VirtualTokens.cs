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

namespace Aurora.Addon.VirtualTokens
{
    public class VirtualToken
    {
        public UUID id = UUID.Zero;
        public string code = "";
        public int estate = 1;
        public UUID founder = UUID.Zero;
        public DateTime created = null;
        public UUID icon = UUID.Zero;
        public bool overridable = false;
        public UUID category = UUID.Zero;
        public string name = "";
        public string description = "";
        public bool enabled = true;
    }

    public class VirtualTokenIssuer
    {
        public UUID tokenID = UUID.Zero;
        public UUID userID = UUID.Zero;
        public bool canIssueChildTokens = false;
        public bool enabled;
    }


    public class VirtualTokens : IAuroraDataPlugin
    {
        private bool m_enabled = false;
        public bool Enabled
        {
            get { return m_enabled; }
        }

        private string defaultIssuerPassword;
        private string defaultIssuerName;
        private UUID defaultIssuerUUID;
        UserAccount defaultIssuer;

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
                return "VirtualTokens";
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

            defaultIssuerName = config.GetString("DefaultIssuerName", "Token Issuer");
            defaultIssuerUUID = UUID.Parse(config.GetString("DefaultIssuerUUID", UUID.Zero.ToString()));
            defaultIssuerPassword = config.GetString("DefaultIssuerPassword", string.Empty);

            if (defaultIssuerPassword == string.Empty)
            {
                m_enabled = false;
                Warn("No password specified for default issuer.");
            }
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

            IUserAccountService userservice = simBase.RequestModuleInterface<IUserAccountService>();
            if (userservice == null)
            {
                m_enabled = false;
                Warn("Could not find user service");
                return;
            }
            if(defaultIssuerUUID != UUID.Zero){
                defaultIssuer = userservice.GetUserAccount(UUID.Zero, defaultIssuerUUID);
            }else{
                defaultIssuer = userservice.GetUserAccount(UUID.Zero, defaultIssuerName);
            }
            if (defaultIssuer == null)
            {
                Warn("Creating default issuer, " + defaultIssuerName);
                IUserAccountService accountService = simBase.RequestModuleInterface<IUserAccountService>();
                defaultIssuerUUID = defaultIssuerUUID == UUID.Zero ? UUID.Random() : defaultIssuerUUID;
                accountService.CreateUser(defaultIssuerUUID, defaultIssuerName, Utils.MD5String(defaultIssuerPassword), string.Empty);
                defaultIssuer = simBase.RequestModuleInterface<IUserAccountService>().GetUserAccount(UUID.Zero, defaultIssuerUUID);
            }

            if (defaultIssuer == null)
            {
                m_enabled = false;
                Warn("Failed to locate or create default issuer.");
                return;
            }

            GD = GenericData;
            GD.ConnectToDatabase(ConnectionString, Name, true);
        }

        #endregion

        #region Tokens

        public VirtualToken AddToken(VirtualToken token)
        {
            token.id = token.id == UUID.Zero ? UUID.Random() : token.id;
            token.created = token.created == null ? DateTime.Now : token.created;

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

        private List<VirtualToken> Query2VirtualToken(List<string> query)
        {
            List<VirtualToken> tokens = new List<VirtualToken>(query.Count % 8 == 0 ? query.Count : 0);

            if (query.Count % 8 != 0)
            {
                Warn("Query result had incorrect number of results. Multiple of 8 expected, found " + query.Count.ToString());
            }
            else
            {
                for (int i = 0; i < query.Count; i += 8)
                {
                    tokens.Add(new VirtualToken
                    {
                        id = UUID.Parse(query[i].ToString()),
                        code = query[i + 1].ToString(),
                        estate = int.Parse(query[i + 2].ToString()),
                        founder = UUID.Parse(query[i + 3].ToString()),
                        created = Utils.UnixTimeToDateTime(int.Parse(query[i + 4].ToString())),
                        icon = UUID.Parse(query[i + 5].ToString()),
                        overridable = uint.Parse(query[i + 6].ToString()) >= 1,
                        category = UUID.Parse(query[i + 7].ToString())
                    });
                }
            }

            return tokens;
        }

        public VirtualToken Get(UUID id)
        {
            VirtualToken token = null;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["id"] = id;
            List<VirtualToken> query = Query2VirtualToken(GD.Query(new string[1] { "*" }, "as_virtualtokens", filter, null, 0, 1));

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

        public VirtualToken Get(string code, int estateID)
        {
            VirtualToken token = null;

            QueryFilter filter = new QueryFilter();
            filter.andFilters["code"] = code;
            filter.andFilters["estate"] = estateID;
            List<VirtualToken> query = Query2VirtualToken(GD.Query(new string[1] { "*" }, "as_virtualtokens", filter, null, 0, 1));

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

        public List<VirtualToken> GetList(string code)
        {
            QueryFilter filter = new QueryFilter();
            filter.andFilters["code"] = code;
            return Query2VirtualToken(GD.Query(new string[1] { "*" }, "as_virtualtokens", filter, null, null, null));
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
            return GD.Update("as_virtualtokens_issuers", new object[2]{
                issuer.canIssueChildTokens ? 1 : 0,
                issuer.enabled ? 1 : 0
            }, new string[2]{
                "issueChildTokens",
                "enabled"
            }, new string[2]{
                "currency",
                "issuer"
            }, new object[2]{
                issuer.tokenID,
                issuer.userID
            });
        }

        #endregion
    }
}
