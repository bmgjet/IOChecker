namespace Oxide.Plugins
{
    [Info("IOChecker", "bmgjet", "1.0.0")]
    [Description("Checks output of card reader IO before taking condition from it")]
    class IOChecker : RustPlugin
    {
        object OnCardSwipe(CardReader cardReader, Keycard card, BasePlayer player)
        {
            //Gets card used
            Item item = card.GetItem();
            //Default card logic if can open door
            if (item != null && card.accessLevel == cardReader.accessLevel && item.conditionNormalized > 0f)
            {
                //Open door
                cardReader.Invoke(new System.Action(cardReader.GrantCard), 0.5f);
                //Check 1 sec after if everything on output path has been powered
                timer.Once(1f,() =>
                {
                    bool RemoveCondition = true;
                    System.Collections.Generic.List<IOEntity> FoundIO = FollowedIO(cardReader as IOEntity);
                    //Checks list isnt empty, if its empty there nothing hooked up to output
                    if (FoundIO == null || FoundIO.Count == 0)
                    {
                        RemoveCondition = false;
                    }
                    else
                    {
                        foreach (IOEntity io in FollowedIO(cardReader as IOEntity))
                        {
                            if (!io.IsPowered())
                            {
                                //Unpowered entity found so dont remove conidtion
                                RemoveCondition = false;
                            }
                        }
                    }
                    //Only remove condition if they are all powered
                    if (RemoveCondition) { item.LoseCondition(1f); }
                });
                //Over ride default function
                return true;
            }
            //Default fail
            cardReader.Invoke(new System.Action(cardReader.FailCard), 0.5f);
            //Over ride default function
            return true;
        }

        private System.Collections.Generic.List<IOEntity> FollowedIO(IOEntity io)
        {
            //Create List
            System.Collections.Generic.List<IOEntity> Connections = new System.Collections.Generic.List<IOEntity>();
            //Check given IO is not null
            if (io.outputs[0].connectedTo != null)
            {
                //Loop though output path and build list
                while (io != null)
                {
                    try
                    {
                        if (io.outputs[0] == null) io = null;
                        if (io.outputs[0].connectedTo == null) io = null;
                        if (io.outputs[0].connectedTo.entityRef.uid == 0) io = null;
                        if (io == null) break;
                        io = BaseNetworkable.serverEntities.Find(io.outputs[0].connectedTo.entityRef.uid) as IOEntity;
                        Connections.Add(io);
                    }
                    catch { break; }
                }
            }
            //return list
            return Connections;
        }
    }
}