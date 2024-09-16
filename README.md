## About this tool

Dead Letter Queue Helper is a lightweight tool to help handle dead letters in an Azure Service Bus Queue.
DLQ's are primitive by design. They do not have the concept of resubmitting a failed message. Azure Portal's Service Bus Explorer introduces a "Resubmit" function, but using it leaves one wanting more, because the only thing it does is send a copy of the message to the queue.

This tool implements another layer on top of the DLQ, where dead letters with the same MessageId are assumed to be attempts/retries of the same message. It allows you to submit another attempt, grouping it with earlier attempts under the same message. The tool polls the queue to find out if the new attempt succeeded or failed, and if it succeeded the tool clears the other attempts from the DLQ.

Dead Letter Queue Helper calls the Azure Service Bus directly from your browser, with your own Entra Id credentials. It does not have any API of its own, nor any remote storage. It is totally local to your browser, speaking only to your service bus. It uses LocalStorage to remember what queues you select, and to remember what messages we are polling for success/failure. It uses the DLQ in a non-intrusive way, adding no metadata to the messages.

As an effect of having no real backend or database, **the tool only works as long as you keep the page open**.

When you login for the first time, you may be greeted with an _"Approval required"_ screen. You may need your admin's approval to do it.

The app asks for permission to _"Have full access to Azure Service Bus service"_*. Unfortunately, I haven't been able to find any more fine-grained permissions for the Service Bus. This tool only requires your account to have _"Azure Service Bus Receiver"_ and _"Azure Service Bus Sender"_ roles on the queues you add.

This website is served by an Azure Static Web App and is made with Blazor.

\* Furthermore, the description for that permission is _"Allow the application full access to the Azure Key Vault service on behalf of the signed-in user"_, which I suspect is a mistake on Microsoft's side.
