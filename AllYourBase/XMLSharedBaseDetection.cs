using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using Verse;

namespace AllYourBase
{
    //using JetBrains.Annotations;

    [StaticConstructorOnStartup]
    //[UsedImplicitly]
    internal static class XmlSharedBaseDetection
    {
        /// <summary>
        /// INTENT: Decrease compatibility issues caused by modders overwriting (abstract) bases or defNames, by making it easy to detect the existence of these bad practices.
        /// 
        /// Run once on startup. Continually adds all Name attributes and defNames into a list to compare against other entries.
        /// The only limit this has, is it does not account for defNames used in abstract Defs
        /// </summary>
        static XmlSharedBaseDetection() {
            //since some people have such wonderfully organised mod lists they hit the 1k limit on error logging:
            Log.ResetMessageCount();

            //get all bases in vanilla.
            HashSet<string> listXMLAttributes = new HashSet<string>();
            HashSet<string> listDefNames = new HashSet<string>();

            foreach (ModContentPack mod in LoadedModManager.RunningMods) {
                foreach (LoadableXmlAsset asset in DirectXmlLoader.XmlAssetsInModFolder(mod, "Defs/")) {
                    if (asset.xmlDoc?.DocumentElement == null) continue;
                    XmlNodeList childNodes = asset.xmlDoc.DocumentElement.ChildNodes;
                    {
                        for (int i = 0; i < childNodes.Count; i++) {
                            if (childNodes[i].NodeType != XmlNodeType.Element) continue;
                            string nameAttribute = childNodes[i]?.Attributes?["Name"]?.Value;
                            if (nameAttribute != null) {
                                if (!listXMLAttributes.Add(nameAttribute)) {
                                    Log.Error("[" + asset.mod.Name + "]" +
                                              " can cause inheritance errors by overwriting attribute " +
                                              nameAttribute + " in file " + asset.FullFilePath);
                                }
                            }

                            if (childNodes[i]?.Attributes?["Abstract"]?.Value != null) {
                                continue;
                            }

                            string itemDefName = childNodes[i]?.SelectSingleNode("defName")?.InnerText;
                            if (itemDefName != null) {
                                if (!listDefNames.Add(childNodes[i].Name + "_" + itemDefName)) {
                                    Log.Error("[" + asset.mod.Name + "]" +
                                              " can cause compatibility errors by overwriting " +
                                              childNodes[i].Name + " " +
                                              itemDefName + " in file " + asset.FullFilePath);
                                }
                            }
                        }
                    }
                }
            }
            
        }
    }
}