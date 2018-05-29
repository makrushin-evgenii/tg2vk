using System;
using System.Linq;
using VkNet.Exception;
using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
  /// <summary>
  /// Информация о медиавложении в записи.
  /// См. описание http://vk.com/dev/attachments_w
  /// </summary>
  [Serializable]
  public class Update
  {
    private EditMessage EditMessage { get; set; }
    private FriendBecameOffline FriendBecameOffline { get; set; }
    private FriendBecameOnline FriendBecameOnline { get; set; }
    private NewMessage NewMessage { get; set; }
    private ReadAllIncomingMessages ReadAllIncomingMessages {get; set;}
    private ReadAllOutcomingMessages ReadAllOutcomingMessages {get; set;}
    private UserTypingInPrivateDialog UserTypingInPrivateDialog {get; set;}

    /// <summary>Экземпляр самого прикрепления.</summary>
    public object Instance
    {
      get
      {
        if (this.Type == typeof (EditMessage))
          return (object) this.EditMessage;
        if (this.Type == typeof (FriendBecameOffline))
          return (object) this.FriendBecameOffline;
        if (this.Type == typeof (FriendBecameOnline))
          return (object) this.FriendBecameOnline;
        if (this.Type == typeof (NewMessage))
          return (object) this.NewMessage;
        if (this.Type == typeof(ReadAllIncomingMessages))
          return (object) this.ReadAllIncomingMessages;
        if (this.Type == typeof(ReadAllOutcomingMessages))
          return (object) this.ReadAllOutcomingMessages;
        if (this.Type == typeof(UserTypingInPrivateDialog))
          return (object) this.UserTypingInPrivateDialog;
        
        return (object) null;
      }
    }

    /// <summary>Информация о типе вложения.</summary>
    public Type Type { get; set; }

    /// <summary>Разобрать из json.</summary>
    /// <param name="response">Ответ сервера.</param>
    /// <returns></returns>
    public static Update FromJson(VkResponseArray response)
    {
      var update = new Update();
      var code = (int) response.First();
      switch (code)
      {
        case 4:
          update.Type = typeof (NewMessage);
          update.NewMessage = NewMessage.FromJson(response);
          break;
        case 5:
          update.Type = typeof (EditMessage);
          update.EditMessage = EditMessage.FromJson(response);
          break;
        case 6:
          update.Type = typeof (ReadAllIncomingMessages);
          update.ReadAllIncomingMessages = ReadAllIncomingMessages.FromJson(response);
          break;
        case 7:
          update.Type = typeof (ReadAllOutcomingMessages);
          update.ReadAllOutcomingMessages = ReadAllOutcomingMessages.FromJson(response);
          break;
        case 8:
          update.Type = typeof (FriendBecameOnline);
          update.FriendBecameOnline = FriendBecameOnline.FromJson(response);
          break;
        case 9:
          update.Type = typeof (FriendBecameOffline);
          update.FriendBecameOffline = FriendBecameOffline.FromJson(response);
          break;
        case 61:
          update.Type = typeof (UserTypingInPrivateDialog);
          update.UserTypingInPrivateDialog = UserTypingInPrivateDialog.FromJson(response);
          break;
        default:
          throw new InvalidParameterException($"The type '{(object) code}' of event is not defined.");
      }
      return update;
    }
  }
}