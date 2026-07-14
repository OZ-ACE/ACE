public enum DialoguePlaySource
{
    None = 0,
    Opening,
    Episode,
    Battle
}

public class DialoguePlayContext
{
    public DialoguePlaySource Source { get; private set; }
    public string SourceDataId { get; private set; }
    public EpisodePlayMode EpisodePlayMode { get; private set; }

    public DialoguePlayContext(DialoguePlaySource source, string sourceDataId, EpisodePlayMode episodePlayMode = EpisodePlayMode.None)
    {
        Source = source;
        SourceDataId = sourceDataId;
        EpisodePlayMode = episodePlayMode;
    }
}