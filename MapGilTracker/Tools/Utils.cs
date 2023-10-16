using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGilTracker.Tools
{
    public class Utils
    {
        /*
         * This code is based off of a snippet I found from SimpleTweaks. Thanks Caraxi!
         * https://github.com/Caraxi/SimpleTweaksPlugin/blob/64ddffadb747c172f6d6294897d7209ca3b39ef6/Utility/Common.cs#L295
         */
        public static unsafe AtkResNode* GetChildNodeByID(AtkResNode* node, uint nodeId) => GetChildNodeByID(node->GetComponent(), nodeId);
        public static unsafe AtkResNode* GetChildNodeByID(AtkComponentBase* component, uint nodeId) => GetChildNodeByID(&component->UldManager, nodeId);
        public static unsafe AtkResNode* GetChildNodeByID(AtkUldManager* uldManager, uint nodeId)
        {
            for (var i = 0; i < uldManager->NodeListCount; i++)
            {
                var n = uldManager->NodeList[i];
                if (n->NodeID != nodeId) continue;
                return n;
            }
            return null;
        }

        /*
         * Code used to traverse the Reward Addon
         */
        public unsafe static int GetIntFromFateReward(AtkUnitBase* addon)
        {
            // Get Base Component node
            var gilSlot = addon->GetNodeById(14);

            // Get child TextNineGrid
            var gilGridNode = GetChildNodeByID(gilSlot, 4);

            // Get child TextNode as Text Node
            var gilTextNode = GetChildNodeByID(gilGridNode, 2)->GetAsAtkTextNode();

            // Parse value and return
            var gilTextValue = gilTextNode->NodeText.ToString();
            return int.Parse(gilTextValue, NumberStyles.AllowThousands);
        }
    }
}
