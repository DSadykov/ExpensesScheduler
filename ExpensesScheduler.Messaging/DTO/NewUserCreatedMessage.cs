using System;
using System.Collections.Generic;
using System.Text;

using ExpensesScheduler.Messaging.Consumer;

namespace ExpensesScheduler.Messaging.DTO;

public class NewUserCreatedMessage
{
    public required Guid UserID {  get; set; }
    public required string Email { get; set; }
}
