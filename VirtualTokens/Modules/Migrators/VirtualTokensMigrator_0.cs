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
using C5;

using Aurora.Framework;
using Aurora.DataManager.Migration;

namespace Aurora.Addon.VirtualTokens.Migrators
{
    public class VirtualTokens_0 : Migrator
    {
        public VirtualTokens_0()
        {
            Version = new Version(0, 0, 0);
            MigrationName = "VirtualTokens";

            schema = new List<Rec<string, ColumnDefinition[], IndexDefinition[]>>(6);

            #region as_virtualtokens
            schema.Add(new Rec<string,ColumnDefinition[],IndexDefinition[]>(
                "as_virtualtokens",
                new ColumnDefinition[11]{
                    new ColumnDefinition{
                        Name = "id",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "code",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 5
                        }
                    },
                    new ColumnDefinition{
                        Name = "estate",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    },
                    new ColumnDefinition{
                        Name = "founder",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "created",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    },
                    new ColumnDefinition{
                        Name = "icon",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "overridable",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Boolean
                        }
                    },
                    new ColumnDefinition{
                        Name = "category",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "enabled",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Boolean
                        }
                    },
                    new ColumnDefinition{
                        Name = "name",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 50
                        }
                    },
                    new ColumnDefinition{
                        Name = "description",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 255
                        }
                    }
                }, new IndexDefinition[2]{
                    new IndexDefinition{
                        Fields = new string[1]{ "id" },
                        Type = IndexType.Primary
                    },
                    new IndexDefinition{
                        Fields = new string[2]{ "code", "estate" },
                        Type = IndexType.Unique
                    }
                }
            ));
            #endregion

            #region as_virtualtokens_category

            schema.Add(new Rec<string, ColumnDefinition[], IndexDefinition[]>(
                "as_virtualtokens_category",
                new ColumnDefinition[5]{
                    new ColumnDefinition{
                        Name = "id",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "parent",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "name",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 50
                        }
                    },
                    new ColumnDefinition{
                        Name = "description",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 255
                        }
                    },
                    new ColumnDefinition{
                        Name = "created",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    }
                }, new IndexDefinition[2]{
                    new IndexDefinition{
                        Fields = new string[1]{ "id" },
                        Type = IndexType.Primary
                    },
                    new IndexDefinition{
                        Fields = new string[1]{ "name" },
                        Type = IndexType.Index
                    }
                }
            ));

            #endregion

            #region as_virtualtokens_issuers

            schema.Add(new Rec<string, ColumnDefinition[], IndexDefinition[]>(
                "as_virtualtokens_issuers",
                new ColumnDefinition[4]{
                    new ColumnDefinition{
                        Name = "currency",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "issuer",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "issueChildTokens",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Boolean
                        }
                    },
                    new ColumnDefinition{
                        Name = "enabled",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Boolean
                        }
                    }
                },
                new IndexDefinition[1]{
                    new IndexDefinition{
                        Fields = new string[2]{ "currency", "issuer" },
                        Type = IndexType.Primary
                    }
                }
            ));

            #endregion

            #region as_virtualtokens_balances

            schema.Add(new Rec<string, ColumnDefinition[], IndexDefinition[]>(
                "as_virtualtokens_balances",
                new ColumnDefinition[3]{
                    new ColumnDefinition{
                        Name = "currency",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "user",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "balance",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    }
                },
                new IndexDefinition[3]{
                    new IndexDefinition{
                        Fields = new string[2]{ "currency", "user" },
                        Type = IndexType.Primary
                    },
                    new IndexDefinition{
                        Fields = new string[3]{ "user", "currency", "balance" },
                        Type = IndexType.Index
                    },
                    new IndexDefinition{
                        Fields = new string[3]{ "currency", "user", "balance" },
                        Type = IndexType.Index
                    }
                }
            ));

            #endregion

            #region as_virtualtokens_transactions

            schema.Add(new Rec<string, ColumnDefinition[], IndexDefinition[]>(
                "as_virtualtokens_transactions",
                new ColumnDefinition[9]{
                    new ColumnDefinition{
                        Name = "id",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "currency",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "sender",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "recipient",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.UUID
                        }
                    },
                    new ColumnDefinition{
                        Name = "issuedOn",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    },
                    new ColumnDefinition{
                        Name = "type",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.TinyInt,
                            Size = 1
                        }
                    },
                    new ColumnDefinition{
                        Name = "amount",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Integer,
                            Size = 11
                        }
                    },
                    new ColumnDefinition{
                        Name = "verified",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.Boolean
                        }
                    },
                    new ColumnDefinition{
                        Name = "message",
                        Type = new ColumnTypeDef{
                            Type = ColumnType.String,
                            Size = 50
                        }
                    }
                }, new IndexDefinition[1]{
                    new IndexDefinition{
                        Fields = new string[1]{ "id" },
                        Type = IndexType.Primary
                    }
                }
            ));



            #endregion
        }

        protected override void DoCreateDefaults(IDataConnector genericData)
        {
            EnsureAllTablesInSchemaExist(genericData);
        }

        protected override bool DoValidate(IDataConnector genericData)
        {
            return TestThatAllTablesValidate(genericData);
        }

        protected override void DoMigrate(IDataConnector genericData)
        {
            DoCreateDefaults(genericData);
        }

        protected override void DoPrepareRestorePoint(IDataConnector genericData)
        {
            CopyAllTablesToTempVersions(genericData);
        }
    }
}