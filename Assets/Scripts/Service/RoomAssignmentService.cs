using System.Collections.Generic;

public class RoomAssignmentService
{
    private const string BEDROOM_ROOM_ID = "Room_Sleep";

    private PlayerModel GetCurrentPlayer()
    {
        return SaveManager.Inst.CurrentPlayerModel;
    }

    public bool AssignRoom(string heroId, long roomInstanceId)
    {
        PlayerModel player = GetCurrentPlayer();

        if (player == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(heroId) == true || roomInstanceId == 0)
        {
            return false;
        }

        if (IsHeroAssigned(heroId) == true)
        {
            return false;
        }

        if (IsRoomOccupied(roomInstanceId) == true)
        {
            return false;
        }

        HeroRoomAssignmentModel assignment = new HeroRoomAssignmentModel();
        assignment.HeroId = heroId;
        assignment.RoomInstanceId = roomInstanceId;

        player.HeroRoomAssignments.Add(assignment);
        SaveManager.Inst.RequestSaveData(player);

        return true;
    }

    public bool RemoveAssignment(string heroId)
    {
        PlayerModel player = GetCurrentPlayer();

        if (player == null)
        {
            return false;
        }

        HeroRoomAssignmentModel assignment = GetAssignmentByHero(heroId);

        if (assignment == null)
        {
            return false;
        }

        player.HeroRoomAssignments.Remove(assignment);
        SaveManager.Inst.RequestSaveData(player);

        return true;
    }

    public HeroRoomAssignmentModel GetAssignmentByHero(string heroId)
    {
        PlayerModel player = GetCurrentPlayer();

        if (player == null)
        {
            return null;
        }

        return player.HeroRoomAssignments.Find(assignment => assignment.HeroId == heroId);
    }

    public HeroRoomAssignmentModel GetAssignmentByRoom(long roomInstanceId)
    {
        PlayerModel player = GetCurrentPlayer();

        if (player == null)
        {
            return null;
        }

        return player.HeroRoomAssignments.Find(assignment => assignment.RoomInstanceId == roomInstanceId);
    }

    public bool IsHeroAssigned(string heroId)
    {
        return GetAssignmentByHero(heroId) != null;
    }

    public bool IsRoomOccupied(long roomInstanceId)
    {
        return GetAssignmentByRoom(roomInstanceId) != null;
    }

    public long GetAssignedRoomInstanceId(string heroId)
    {
        HeroRoomAssignmentModel assignment = GetAssignmentByHero(heroId);

        if (assignment == null)
        {
            return 0;
        }

        return assignment.RoomInstanceId;
    }

    public string GetAssignedHeroId(long roomInstanceId)
    {
        HeroRoomAssignmentModel assignment = GetAssignmentByRoom(roomInstanceId);

        if (assignment == null)
        {
            return null;
        }

        return assignment.HeroId;
    }

    public List<PlacedRoomData> GetEmptyRooms()
    {
        List<PlacedRoomData> emptyRooms = new List<PlacedRoomData>();

        BuildGridViewModel buildGridViewModel = GameManager.Inst.Services.BuildService.GetBuildGridViewModel();

        List<PlacedRoomData> rooms = buildGridViewModel.GetPlacedRooms();

        foreach (PlacedRoomData room in rooms)
        {
            if (room.RoomId != BEDROOM_ROOM_ID)
            {
                continue;
            }

            if (IsRoomOccupied(room.RoomInstanceId))
            {
                continue;
            }

            emptyRooms.Add(room);
        }

        return emptyRooms;
    }
}