public class LobbyPageQuest : LobbyPageBase
{
    public PageAchievement Achievement;
    public PageQuest Quest;

    public override void OnShow()
    {
        base.OnShow();

        Quest.Show();
        Achievement.Exit();
    }

    public void OnClickQuest()
    {
        if (Quest.gameObject.activeSelf) return;
        Quest.Show();
        Achievement.Exit();
    }

    public void OnClickAchievement()
    {
        if (Achievement.gameObject.activeSelf) return;
        Quest.Exit();
        Achievement.Show();
    }
}