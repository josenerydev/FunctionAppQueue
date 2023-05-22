## Azure Queue Storage com trigger no Azure Function

Discussão:
1. No consumidor da fila principal `sample-queue` tratamento especial para erros conhecidos. Por exemplo: `TimeOut Exception`.
2. No consumidor da fila principal `sample-queue` tratamento genérico para erros desconhecidos. Por exemplo: Exception genérica.
3. Regras e implementação para reprocessar `TimeOut Exception`, política de retentativa e circuit break.
4. Gerenciamento de `sample-queue-poison`(DLQ).
5. Particularidades do Azure Queue Storage
   - Mensagens na fila <queue>-poison, com tempo "imutável" de vida, 7 dias.
   - Não há suporte oficial para a personalização da política de retentativa em Azure Functions com triggers de fila.
   - Quando uma mensagem é movida para a fila de poison, ela recebe um novo ID de mensagem.
6. Implementação de uma abordagem de controle de concorrência na fila, para garantir que múltiplos consumidores possam operar na fila sem sobreposição.
7. Ordem das mensagens: Como a Azure Queue Storage não garante a ordem das mensagens, discutir estratégias para lidar com isso.
8. Garantias de entrega: Em termos de garantia de entrega "pelo menos uma vez", o Azure Queue Storage oferece essa garantia, já que, se uma mensagem não for processada com sucesso, ela será retentada até ser movida para a fila DLQ. No entanto, pode haver duplicação de mensagens se o consumidor processar a mensagem com sucesso, mas falhar ao confirmar a exclusão da mensagem para a fila. Isso resultaria na mesma mensagem sendo processada mais de uma vez quando ela se tornasse visível novamente após o tempo de visibilidade expirar. Portanto, os consumidores devem ser projetados para serem idempotentes.