using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class EntityRelocate : WorldEvent
{
    public ActionsForEntities Action { get; set; } // legends_plus.xml
    public Entity? Entity { get; set; }
    public Site? Site { get; set; }
    public int StructureId { get; set; }
    public Structure? Structure { get; set; }

    public EntityRelocate(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "entity_id": Entity = world.GetEntity(Convert.ToInt32(property.Value)); break;
                case "site_id": Site = world.GetSite(Convert.ToInt32(property.Value)); break;
                case "structure_id": StructureId = Convert.ToInt32(property.Value); break;
                case "site": if (Site == null) { Site = world.GetSite(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "entity": if (Entity == null) { Entity = world.GetEntity(Convert.ToInt32(property.Value)); } else { property.Known = true; } break;
                case "action":
                    switch (property.Value)
                    {
                        case "entity_relocate":
                            Action = ActionsForEntities.Relocate;
                            break;
                        default:
                            property.Known = false;
                            break;
                    }
                    break;
                case "structure": StructureId = Convert.ToInt32(property.Value); break;
            }
        }
        if (Site != null)
        {
            Structure = Site.Structures.Find(structure => structure.LocalId == StructureId);
        }
        Entity.AddEvent(this);
        Site.AddEvent(this);
        Structure.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime() + Entity?.ToLink(link, pov, this) + " moved to ";
        if (Structure != null)
        {
            eventString += Structure.ToLink(link, pov, this);
        }
        else
        {
            eventString += "UNKNOWN STRUCTURE";
        }
        eventString += " in " + Site?.ToLink(link, pov, this);
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}