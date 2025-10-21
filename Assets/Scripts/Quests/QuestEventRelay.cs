using UnityEngine;

/// <summary>
/// Convenience component that exposes quest manager callbacks to UnityEvents and timeline signals.
/// Attach this script to quest-giving objects or story controllers and wire its public methods from the inspector.
/// </summary>
public class QuestEventRelay : MonoBehaviour
{
    public void ReportCampfireStayedLit()
    {
        QuestManager.instance?.ReportCampfireNight(true);
    }

    public void ReportCampfireExtinguished()
    {
        QuestManager.instance?.ReportCampfireNight(false);
    }

    public void ReportBlueprintAcquired(string blueprintId)
    {
        if (!string.IsNullOrEmpty(blueprintId))
        {
            QuestManager.instance?.RegisterBlueprintAcquired(blueprintId);
        }
    }

    public void ReportStructureProgress(string structureId)
    {
        if (!string.IsNullOrEmpty(structureId))
        {
            QuestManager.instance?.RegisterStructureProgress(structureId);
        }
    }

    public void ReportBoatAssembled(string boatId)
    {
        QuestManager.instance?.RegisterBoatAssembled(boatId);
    }

    public void ReportTradeCompleted(string tradeId)
    {
        if (!string.IsNullOrEmpty(tradeId))
        {
            QuestManager.instance?.RegisterTrade(tradeId);
        }
    }

    public void ReportExploration(string pointId)
    {
        if (!string.IsNullOrEmpty(pointId))
        {
            QuestManager.instance?.RegisterExplorationPoint(pointId);
        }
    }

    public void ReportInteraction(string interactionId)
    {
        if (!string.IsNullOrEmpty(interactionId))
        {
            QuestManager.instance?.RegisterInteraction(interactionId);
        }
    }
}
