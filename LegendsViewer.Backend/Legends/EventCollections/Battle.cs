﻿using LegendsViewer.Backend.Legends.Enums;
using LegendsViewer.Backend.Legends.Events;
using LegendsViewer.Backend.Legends.Extensions;
using LegendsViewer.Backend.Legends.IncidentalEvents;
using LegendsViewer.Backend.Legends.Interfaces;
using LegendsViewer.Backend.Legends.Parser;
using LegendsViewer.Backend.Legends.Various;
using LegendsViewer.Backend.Legends.WorldObjects;
using LegendsViewer.Backend.Utilities;

namespace LegendsViewer.Backend.Legends.EventCollections;

public class Battle : EventCollection, IHasComplexSubtype
{
    public BattleOutcome Outcome { get; set; }
    public Location? Coordinates { get; set; }
    public SiteConquered? Conquering { get; set; }
    public Entity? Attacker { get; set; }
    public Entity? Defender { get; set; }
    public Entity? Victor { get; set; }
    public List<Squad> Attackers { get; set; } = [];
    public List<Squad> Defenders { get; set; } = [];
    public List<HistoricalFigure> NotableAttackers { get; set; } = [];
    public List<HistoricalFigure> NotableDefenders { get; set; } = [];
    public List<HistoricalFigure> NonCombatants { get; set; } = [];
    public List<Squad> AttackerSquads { get; set; } = [];
    public List<Squad> DefenderSquads { get; set; } = [];
    public int AttackerCount => NotableAttackers.Count + AttackerSquads.Sum(squad => squad.Numbers);
    public int DefenderCount => NotableDefenders.Count + DefenderSquads.Sum(squad => squad.Numbers);
    public int AttackersRemainingCount => Attackers.Sum(squad => squad.Numbers - squad.Deaths);
    public int DefendersRemainingCount => Defenders.Sum(squad => squad.Numbers - squad.Deaths);
    public int DeathCount => AttackerDeathCount + DefenderDeathCount;
    public Dictionary<CreatureInfo, int> Deaths { get; set; } = [];
    public List<HistoricalFigure> NotableDeaths => NotableAttackers
        .Where(attacker => GetSubEvents().OfType<HfDied>()
        .Count(death => death.HistoricalFigure == attacker) > 0)
        .Concat(NotableDefenders.Where(defender => GetSubEvents().OfType<HfDied>().Any(death => death.HistoricalFigure == defender)))
        .ToList();
    public int AttackerDeathCount { get; set; }
    public int DefenderDeathCount { get; set; }
    public double AttackersToDefenders
    {
        get
        {
            if (AttackerCount == 0 && DefenderCount == 0)
            {
                return 0;
            }

            return DefenderCount == 0 ? double.MaxValue : Math.Round(AttackerCount / Convert.ToDouble(DefenderCount), 2);
        }
        set { }
    }
    public double AttackersToDefendersRemaining
    {
        get
        {
            if (AttackersRemainingCount == 0 && DefendersRemainingCount == 0)
            {
                return 0;
            }

            return DefendersRemainingCount == 0
                ? double.MaxValue
                : Math.Round(AttackersRemainingCount / Convert.ToDouble(DefendersRemainingCount), 2);
        }
        set { }
    }

    public bool IndividualMercenaries { get; set; }
    public bool CompanyMercenaries { get; set; }
    public Entity? AttackingMercenaryEntity { get; set; }
    public Entity? DefendingMercenaryEntity { get; set; }
    public bool AttackingSquadAnimated { get; set; }
    public bool DefendingSquadAnimated { get; set; }
    public List<Entity> AttackerSupportMercenaryEntities { get; set; } = [];
    public List<Entity> DefenderSupportMercenaryEntities { get; set; } = [];
    public List<HistoricalFigure> AttackerSupportMercenaryHfs { get; set; } = [];
    public List<HistoricalFigure> DefenderSupportMercenaryHfs { get; set; } = [];

    private WorldRegion? _region;

    public Battle(List<Property> properties, World world)
        : base(properties, world)
    {
        var attackerSquadRaces = new List<CreatureInfo>();
        var attackerSquadEntityPopulation = new List<int>();
        var attackerSquadNumbers = new List<int>();
        var attackerSquadDeaths = new List<int>();
        var attackerSquadSite = new List<int>();
        var defenderSquadRaces = new List<CreatureInfo>();
        var defenderSquadEntityPopulation = new List<int>();
        var defenderSquadNumbers = new List<int>();
        var defenderSquadDeaths = new List<int>();
        var defenderSquadSite = new List<int>();
        foreach (Property property in properties)
        {
            switch (property.Name)
            {
                case "outcome":
                    switch (property.Value)
                    {
                        case "attacker won": Outcome = BattleOutcome.AttackerWon; break;
                        case "defender won": Outcome = BattleOutcome.DefenderWon; break;
                        default: Outcome = BattleOutcome.Unknown; world.ParsingErrors.Report("Unknown Battle Outcome: " + property.Value); break;
                    }
                    break;
                case "name": Name = Formatting.InitCaps(property.Value); break;
                case "coords": Coordinates = Formatting.ConvertToLocation(property.Value); break;
                case "war_eventcol": ParentCollection = world.GetEventCollection(Convert.ToInt32(property.Value)); break;
                case "attacking_hfid":
                    HistoricalFigure? attackingHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if (attackingHf != null)
                    {
                        NotableAttackers.Add(attackingHf);
                    }
                    break;
                case "defending_hfid":
                    HistoricalFigure? defendingHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if (defendingHf != null)
                    {
                        NotableDefenders.Add(defendingHf);
                    }
                    break;
                case "attacking_squad_race": attackerSquadRaces.Add(world.GetCreatureInfo(property.Value)); break;
                case "attacking_squad_entity_pop": attackerSquadEntityPopulation.Add(Convert.ToInt32(property.Value)); break;
                case "attacking_squad_number":
                    int attackerSquadNumber = Convert.ToInt32(property.Value);
                    attackerSquadNumbers.Add(attackerSquadNumber < 0 || attackerSquadNumber > Squad.MAX_SIZE ? Squad.MAX_SIZE : attackerSquadNumber);
                    break;
                case "attacking_squad_deaths":
                    int attackerSquadDeath = Convert.ToInt32(property.Value);
                    attackerSquadDeaths.Add(attackerSquadDeath < 0 || attackerSquadDeath > Squad.MAX_SIZE ? Squad.MAX_SIZE : attackerSquadDeath);
                    break;
                case "attacking_squad_site": attackerSquadSite.Add(Convert.ToInt32(property.Value)); break;
                case "defending_squad_race": defenderSquadRaces.Add(world.GetCreatureInfo(property.Value)); break;
                case "defending_squad_entity_pop": defenderSquadEntityPopulation.Add(Convert.ToInt32(property.Value)); break;
                case "defending_squad_number":
                    int defenderSquadNumber = Convert.ToInt32(property.Value);
                    defenderSquadNumbers.Add(defenderSquadNumber < 0 || defenderSquadNumber > Squad.MAX_SIZE ? Squad.MAX_SIZE : defenderSquadNumber);
                    break;
                case "defending_squad_deaths":
                    int defenderSquadDeath = Convert.ToInt32(property.Value);
                    defenderSquadDeaths.Add(defenderSquadDeath < 0 || defenderSquadDeath > Squad.MAX_SIZE ? Squad.MAX_SIZE : defenderSquadDeath);
                    break;
                case "defending_squad_site": defenderSquadSite.Add(Convert.ToInt32(property.Value)); break;
                case "noncom_hfid":
                    HistoricalFigure? nonCombatantHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if (nonCombatantHf != null)
                    {
                        NonCombatants.Add(nonCombatantHf);
                    }
                    break;
                case "individual_merc": property.Known = true; IndividualMercenaries = true; break;
                case "company_merc": property.Known = true; CompanyMercenaries = true; break;
                case "attacking_merc_enid":
                    AttackingMercenaryEntity = world.GetEntity(Convert.ToInt32(property.Value));
                    //if (AttackingMercenaryEntity != null)
                    //{
                    //    AttackingMercenaryEntity.Type = EntityType.MercenaryCompany;
                    //}
                    break;
                case "defending_merc_enid":
                    DefendingMercenaryEntity = world.GetEntity(Convert.ToInt32(property.Value));
                    //if (DefendingMercenaryEntity != null)
                    //{
                    //    DefendingMercenaryEntity.Type = EntityType.MercenaryCompany;
                    //}
                    break;
                case "attacking_squad_animated": property.Known = true; AttackingSquadAnimated = true; break;
                case "defending_squad_animated": property.Known = true; DefendingSquadAnimated = true; break;
                case "a_support_merc_enid":
                    var attackerSupportMercenaryEntity = world.GetEntity(Convert.ToInt32(property.Value));
                    if (attackerSupportMercenaryEntity != null)
                    {
                        AttackerSupportMercenaryEntities.Add(attackerSupportMercenaryEntity);
                        //attackerSupportMercenaryEntity.Type = EntityType.MercenaryCompany;
                    }
                    break;
                case "d_support_merc_enid":
                    var defenderSupportMercenaryEntity = world.GetEntity(Convert.ToInt32(property.Value));
                    if (defenderSupportMercenaryEntity != null)
                    {
                        DefenderSupportMercenaryEntities.Add(defenderSupportMercenaryEntity);
                        //defenderSupportMercenaryEntity.Type = EntityType.MercenaryCompany;
                    }
                    break;
                case "a_support_merc_hfid":
                    HistoricalFigure? attackerSupportMercenaryHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if (attackerSupportMercenaryHf != null)
                    {
                        AttackerSupportMercenaryHfs.Add(attackerSupportMercenaryHf);
                    }
                    break;
                case "d_support_merc_hfid":
                    HistoricalFigure? defenderSupportMercenaryHf = world.GetHistoricalFigure(Convert.ToInt32(property.Value));
                    if(defenderSupportMercenaryHf != null)
                    {
                        DefenderSupportMercenaryHfs.Add(defenderSupportMercenaryHf);
                    }
                    break;
            }
        }

        if (Events.OfType<AttackedSite>().Any())
        {
            Attacker = Events.OfType<AttackedSite>().First().Attacker;
            Defender = Events.OfType<AttackedSite>().First().Defender;
        }
        else if (Events.OfType<FieldBattle>().Any())
        {
            Attacker = Events.OfType<FieldBattle>().First().Attacker;
            Defender = Events.OfType<FieldBattle>().First().Defender;
        }

        foreach (HistoricalFigure involvedHf in NotableAttackers.Union(NotableDefenders).Where(hf => hf != HistoricalFigure.Unknown))
        {
            involvedHf.Battles.Add(this);
            involvedHf.AddEventCollection(this);
            involvedHf.AddEvent(new BattleFought(involvedHf, this, World));
        }

        foreach (HistoricalFigure involvedSupportMercenaries in AttackerSupportMercenaryHfs.Union(DefenderSupportMercenaryHfs).Where(hf => hf != HistoricalFigure.Unknown))
        {
            involvedSupportMercenaries.Battles.Add(this);
            involvedSupportMercenaries.AddEventCollection(this);
            involvedSupportMercenaries.AddEvent(new BattleFought(involvedSupportMercenaries, this, World, true, true));
        }

        for (int i = 0; i < attackerSquadRaces.Count; i++)
        {
            AttackerSquads.Add(new Squad(attackerSquadRaces[i], attackerSquadNumbers[i], attackerSquadDeaths[i], attackerSquadSite[i], attackerSquadEntityPopulation[i]));
        }

        for (int i = 0; i < defenderSquadRaces.Count; i++)
        {
            DefenderSquads.Add(new Squad(defenderSquadRaces[i], defenderSquadNumbers[i], defenderSquadDeaths[i], defenderSquadSite[i], defenderSquadEntityPopulation[i]));
        }

        var groupedAttackerSquads = from squad in AttackerSquads
                                    group squad by squad.Race into squadRace
                                    select new { Race = squadRace.Key, Count = squadRace.Sum(squad => squad.Numbers), Deaths = squadRace.Sum(squad => squad.Deaths) };
        foreach (var squad in groupedAttackerSquads)
        {
            int attackerSquadNumber = squad.Count + NotableAttackers.Count(attacker => attacker?.Race?.Id == squad.Race.Id);
            int attackerSquadDeath = squad.Deaths + Events.OfType<HfDied>().Count(death => death.HistoricalFigure?.Race == squad.Race && NotableAttackers.Contains(death.HistoricalFigure));
            Squad attackerSquad = new(squad.Race, attackerSquadNumber, attackerSquadDeath, -1, -1);
            Attackers.Add(attackerSquad);
        }

        foreach (var attacker in NotableAttackers.Where(hf => Attackers.Count(squad => squad.Race == hf.Race) == 0).GroupBy(hf => hf.Race).Select(race => new { Race = race.Key, Count = race.Count() }))
        {
            var attackerDeath = Events.OfType<HfDied>().Count(death => NotableAttackers.Contains(death.HistoricalFigure) && death.HistoricalFigure?.Race == attacker.Race);
            Attackers.Add(new Squad(attacker.Race, attacker.Count, attackerDeath, -1, -1));
        }

        var groupedDefenderSquads = from squad in DefenderSquads
                                    group squad by squad.Race into squadRace
                                    select new { Race = squadRace.Key, Count = squadRace.Sum(squad => squad.Numbers), Deaths = squadRace.Sum(squad => squad.Deaths) };
        foreach (var squad in groupedDefenderSquads)
        {
            int defenderSquadNumber = squad.Count + NotableDefenders.Count(defender => defender?.Race?.Id == squad.Race.Id);
            int defenderSquadDeath = squad.Deaths + Events.OfType<HfDied>().Count(death => death.HistoricalFigure?.Race == squad.Race && NotableDefenders.Contains(death.HistoricalFigure));
            Defenders.Add(new Squad(squad.Race, defenderSquadNumber, defenderSquadDeath, -1, -1));
        }

        foreach (var defender in NotableDefenders.Where(hf => Defenders.Count(squad => squad.Race == hf.Race) == 0).GroupBy(hf => hf.Race).Select(race => new { Race = race.Key, Count = race.Count() }))
        {
            int defenderDeath = Events.OfType<HfDied>().Count(death => NotableDefenders.Contains(death.HistoricalFigure) && death.HistoricalFigure.Race == defender.Race);
            Defenders.Add(new Squad(defender.Race, defender.Count, defenderDeath, -1, -1));
        }

        Deaths = [];
        foreach (Squad squad in Attackers.Concat(Defenders).Where(a => a.Race != null && a.Race != CreatureInfo.Unknown))
        {
            if (Deaths.ContainsKey(squad.Race))
            {
                Deaths[squad.Race] += squad.Deaths;
            }
            else
            {
                Deaths[squad.Race] = squad.Deaths;
            }
        }

        AttackerDeathCount = Attackers.Sum(attacker => attacker.Deaths);
        DefenderDeathCount = Defenders.Sum(defender => defender.Deaths);

        if (Outcome == BattleOutcome.AttackerWon)
        {
            Victor = Attacker;
        }
        else if (Outcome == BattleOutcome.DefenderWon)
        {
            Victor = Defender;
        }

        if (ParentCollection is War parentWar)
        {
            if (parentWar.Attacker == Attacker)
            {
                parentWar.AttackerDeathCount += AttackerDeathCount;
                parentWar.DefenderDeathCount += DefenderDeathCount;
            }
            else
            {
                parentWar.AttackerDeathCount += DefenderDeathCount;
                parentWar.DefenderDeathCount += AttackerDeathCount;
            }
            parentWar.DeathCount += attackerSquadDeaths.Sum() + defenderSquadDeaths.Sum() + Events.OfType<HfDied>().Count();

            if (Attacker == parentWar.Attacker && Victor == Attacker)
            {
                parentWar.AttackerVictories.Add(this);
            }
            else
            {
                parentWar.DefenderVictories.Add(this);
            }
        }
        Region?.Battles.Add(this);
        UndergroundRegion?.Battles.Add(this);

        if (attackerSquadDeaths.Sum() + defenderSquadDeaths.Sum() + Events.OfType<HfDied>().Count() == 0)
        {
            Notable = false;
        }

        if (attackerSquadNumbers.Sum() + NotableAttackers.Count > (defenderSquadNumbers.Sum() + NotableDefenders.Count) * 10 //NotableDefenders outnumbered 10 to 1
            && Victor == Attacker
            && AttackerDeathCount < (NotableAttackers.Count + attackerSquadNumbers.Sum()) * 0.1) //NotableAttackers losses < 10%
        {
            Notable = false;
        }
        Attacker?.AddEventCollection(this);
        if (Defender != Attacker)
        {
            Defender?.AddEventCollection(this);
        }

        Icon = HtmlStyleUtil.GetIconString("chess-bishop");
    }

    public void GenerateComplexSubType()
    {
        if (string.IsNullOrEmpty(Subtype))
        {
            Subtype = $"{Attacker?.ToLink(true, this)}{(Victor != null && Victor == Attacker ? "(V)" : "")} => {Defender?.ToLink(true, this)}{(Victor != null && Victor == Defender ? "(V)" : "")}";
        }
    }

    public class Squad
    {
        public const int MAX_SIZE = 100;
        public CreatureInfo Race { get; set; }
        public int Numbers { get; set; }
        public int Deaths { get; set; }
        public int Site { get; set; }
        public int Population { get; set; }
        public Squad(CreatureInfo race, int numbers, int deaths, int site, int population)
        {
            Race = race;
            Numbers = numbers;
            Deaths = deaths;
            Site = site;
            Population = population;
        }
    }

    public override string ToLink(bool link = true, DwarfObject? pov = null, WorldEvent? worldEvent = null)
    {
        if (link)
        {
            string title = GetTitle();

            string linkedString = pov != this
                ? HtmlStyleUtil.GetAnchorString(Icon, "battle", Id, title, Name)
                : HtmlStyleUtil.GetAnchorCurrentString(Icon, title, HtmlStyleUtil.CurrentDwarfObject(Name));
            return linkedString;
        }
        return Name;
    }

    private string GetTitle()
    {
        string title = Type;
        title += "&#13";
        title += Attacker != null ? Attacker.PrintEntity(false) : "UNKNOWN";
        title += " (Attacker)";
        if (Victor == Attacker)
        {
            title += "(V)";
        }

        title += "&#13";
        title += "Kills: " + DefenderDeathCount;
        title += "&#13";
        title += Defender != null ? Defender.PrintEntity(false) : "UNKNOWN";
        title += " (Defender)";
        if (Victor == Defender)
        {
            title += "(V)";
        }

        title += "&#13";
        title += "Kills: " + AttackerDeathCount;
        return title;
    }

    public override string ToString()
    {
        return Name;
    }

    public override string GetIcon()
    {
        return Icon;
    }
}
