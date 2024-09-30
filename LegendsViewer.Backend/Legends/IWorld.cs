﻿using LegendsViewer.Backend.Legends.EventCollections;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.WorldObjects;
using System.Text.Json.Serialization;

namespace LegendsViewer.Backend.Legends;

public interface IWorld
{
    string Name { get; }
    string AlternativeName { get; }

    int Width { get; set; }
    int Height { get; set; }

    List<WorldRegion> Regions { get; }
    List<UndergroundRegion> UndergroundRegions { get; }
    List<Landmass> Landmasses { get; }
    List<MountainPeak> MountainPeaks { get; }
    List<Identity> Identities { get; }
    List<River> Rivers { get; }
    List<Site> Sites { get; }
    List<HistoricalFigure> HistoricalFigures { get; }
    List<Entity> Entities { get; }
    List<War> Wars { get; }
    List<Battle> Battles { get; }
    List<BeastAttack> BeastAttacks { get; }
    List<Era> Eras { get; }
    List<Artifact> Artifacts { get; }
    List<WorldConstruction> WorldConstructions { get; }
    List<PoeticForm> PoeticForms { get; }
    List<MusicalForm> MusicalForms { get; }
    List<DanceForm> DanceForms { get; }
    List<WrittenContent> WrittenContents { get; }
    List<Structure> Structures { get; }
    List<WorldEvent> Events { get; }
    List<EventCollection> EventCollections { get; }
    List<EntityPopulation> EntityPopulations { get; }
    List<Population> SitePopulations { get; }
    List<Population> CivilizedPopulations { get; }
    List<Population> OutdoorPopulations { get; }
    List<Population> UndergroundPopulations { get; }

    Task ParseAsync(string xmlFile, string? xmlPlusFile, string? historyFile, string? sitesAndPopulationsFile, string? mapFile);
    CreatureInfo GetCreatureInfo(string id);
    Artifact? GetArtifact(int id);
    DanceForm? GetDanceForm(int id);
    Entity? GetEntity(int id);
    EntityPopulation? GetEntityPopulation(int id);
    Era? GetEra(int id);
    WorldEvent? GetEvent(int id);
    EventCollection? GetEventCollection(int id);
    HistoricalFigure? GetHistoricalFigure(int id);
    MusicalForm? GetMusicalForm(int id);
    PoeticForm? GetPoeticForm(int id);
    WorldRegion? GetRegion(int id);
    Site? GetSite(int id);
    Structure? GetStructure(int id);
    UndergroundRegion? GetUndergroundRegion(int id);
    WorldConstruction? GetWorldConstruction(int id);
    WrittenContent? GetWrittenContent(int id);
    Landmass? GetLandmass(int id);
    River? GetRiver(int id);
    MountainPeak? GetMountainPeak(int id);

    void Clear();
}