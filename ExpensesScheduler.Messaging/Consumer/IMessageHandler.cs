using System;
using System.Collections.Generic;
using System.Text;

namespace ExpensesScheduler.Messaging.Consumer;

public interface IMessageHandler<in Message>
{
    Task HandleAsync(Message message, CancellationToken cancellationToken);
}
