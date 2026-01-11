using System;
using System.Collections.Generic;
using System.Text;

namespace ExpensesScheduler.Messaging.DTO;

public class NotificationMessage
{
    public required string UserID {  get; set; }
    public required string Message { get; set; }
    public required double Amount { get; set; }
}
