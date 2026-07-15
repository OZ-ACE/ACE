public class RoomExistsConditionEvaluator : IEpisodeConditionEvaluator
{
    public EpisodeConditionType ConditionType => EpisodeConditionType.RoomExists;

    public bool IsSatisfied(EpisodeConditionData conditionData, PlayerModel playerModel)
    {
        if (conditionData == null || playerModel == null || playerModel.BuildGridData == null || playerModel.BuildGridData.PlacedRooms == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(conditionData.TargetId))
        {
            return false;
        }

        for (int i = 0; i < playerModel.BuildGridData.PlacedRooms.Count; i++)
        {
            PlacedRoomData placedRoomData = playerModel.BuildGridData.PlacedRooms[i];

            if (placedRoomData == null)
            {
                continue;
            }

            if (placedRoomData.RoomId == conditionData.TargetId)
            {
                return true;
            }
        }

        return false;
    }
}