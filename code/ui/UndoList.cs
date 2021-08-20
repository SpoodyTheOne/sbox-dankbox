using Sandbox;
using Sandbox.UI;

[Library]
public partial class UndoList : Panel
{
    public static UndoList Instance;

    public UndoList()
    {
        Instance = this;
        StyleSheet.Load("/ui/UndoList.scss");

        var testmsg = Add.Panel("div");
        {
            testmsg.AddClass("msg");

            var text = testmsg.AddChild<Label>("value");

            text.Text = "Test Message";

            testmsg.AddClass("show");
        }
    }
}