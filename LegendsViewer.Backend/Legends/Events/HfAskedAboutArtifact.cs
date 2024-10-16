﻿using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.WorldObjects;

namespace LegendsViewer.Backend.Legends.Events;

public class HfAskedAboutArtifact : WorldEvent
{
    public Artifact? Artifact { get; set; }
    public HistoricalFigure? HistoricalFigure { get; set; }
    public Site? Site { get; set; }
    public int StructureId { get; set; }
    public Structure? Structure { get; set; }
    public WorldRegion? Region { get; set; }
    public UndergroundRegion? UndergroundRegion { get; set; }

    public HfAskedAboutArtifact(List<Property> properties, World world)
        : base(properties, world)
    {
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "artifact_id":
                    Artifact = world.GetArtifact(Convert.ToInt32(property.Value));
                    break;
                case "hist_fig_id":
                    HistoricalFigure = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    break;
                case "site_id":
                    Site = world.GetSite(Convert.ToInt32(property.Value));
                    break;
                case "structure_id":
                    StructureId = Convert.ToInt32(property.Value);
                    break;
                case "subregion_id": Region = world.GetRegion(Convert.ToInt32(property.Value)); break;
                case "feature_layer_id": UndergroundRegion = world.GetUndergroundRegion(Convert.ToInt32(property.Value)); break;
            }
        }

        if (Site != null)
        {
            Structure = Site.Structures.Find(structure => structure.LocalId == StructureId);
        }
        Artifact.AddEvent(this);
        HistoricalFigure.AddEvent(this);
        Site.AddEvent(this);
        Structure.AddEvent(this);
        Region.AddEvent(this);
        UndergroundRegion.AddEvent(this);
    }

    public override string Print(bool link = true, DwarfObject? pov = null)
    {
        string eventString = GetYearTime();
        eventString += HistoricalFigure?.ToLink(link, pov, this);
        eventString += " asked about ";
        eventString += Artifact?.ToLink(link, pov, this);
        if (Structure != null)
        {
            eventString += " inside ";
            eventString += Structure.ToLink(link, pov, this);
        }
        if (Site != null)
        {
            eventString += " in ";
            eventString += Site.ToLink(link, pov, this);
        }
        else if (Region != null)
        {
            eventString += " in ";
            eventString += Region.ToLink(link, pov, this);
        }
        else if (UndergroundRegion != null)
        {
            eventString += " in ";
            eventString += UndergroundRegion.ToLink(link, pov, this);
        }
        eventString += PrintParentCollection(link, pov);
        eventString += ".";
        return eventString;
    }
}
