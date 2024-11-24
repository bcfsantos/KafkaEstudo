using Confluent.Kafka; // Biblioteca para integração com Kafka
using Confluent.Kafka.SyncOverAsync; // Permite a utilização de métodos síncronos em serializadores assíncronos
using Confluent.SchemaRegistry; // Biblioteca para integração com Schema Registry
using Confluent.SchemaRegistry.Serdes; // Suporte para serialização/deserialização Avro
using System; // Namespace padrão do C#

namespace KafkaEstudo
{
    internal class Program
    {
        // Nome do tópico Kafka onde as mensagens serão publicadas e consumidas
        private const string Topico = "KafkaEstudo";

        // Endereço do servidor Kafka
        private const string BootstrapServer = "localhost:9092";

        // Endereço do servidor Schema Registry (para Avro)
        private const string ServerAvro = "http://localhost:8081";

        /// <summary>
        /// Método principal que inicia o programa.
        /// </summary>
        static async Task Main(string[] args)
        {
            var i = 1; // Contador para diferenciar mensagens enviadas

            // Inicia consumidores em diferentes grupos (cada um com comportamento de consumo único)
            _ = Task.Run(() => ConsumirMensagens("grupo1", AutoOffsetReset.Earliest)); // Consome a partir do início do log
            _ = Task.Run(() => ConsumirMensagens("grupo2", AutoOffsetReset.Latest)); // Consome apenas as mensagens novas

            while (true) // Loop para enviar mensagens
            {
                Console.WriteLine("Pressione Enter para enviar uma mensagem...");
                Console.ReadLine(); // Aguarda o usuário pressionar Enter
                await ProduzirMensagemAsync(i); // Envia a próxima mensagem
                i++; // Incrementa o contador
            }
        }

        /// <summary>
        /// Produz mensagens no tópico Kafka utilizando serialização Avro.
        /// </summary>
        static async Task ProduzirMensagemAsync(int indice)
        {
            // Configuração do Schema Registry para integração com Avro
            var schemaConfig = new SchemaRegistryConfig { Url = ServerAvro };
            using var schemaRegistry = new CachedSchemaRegistryClient(schemaConfig); // Cliente para gerenciar esquemas Avro

            // Configuração do produtor Kafka
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = BootstrapServer, // Servidor Kafka
                EnableIdempotence = true, // Garante que mensagens duplicadas não sejam produzidas
                Acks = Acks.All, // Aguarda confirmação de todas as réplicas antes de considerar a mensagem enviada
                MaxInFlight = 5, // Número máximo de mensagens em voo para evitar inconsistências
                TransactionalId = "transacao-kafka-estudo" // ID da transação para suporte a transações
            };

            try
            {
                // Criação do produtor Kafka com suporte a serialização Avro
                using var producer = new ProducerBuilder<string, Curso>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<Curso>(schemaRegistry)) // Serializa os valores usando Avro
                    .Build();

                // Criação da mensagem que será enviada
                var mensagem = new Curso
                {
                    id = Guid.NewGuid().ToString(), // Gera um identificador único
                    descricao = $"Curso Kafka Avro - {indice}" // Descrição dinâmica com base no índice
                };

                Console.WriteLine($"Produzindo mensagem {indice}...");

                // Inicializa o suporte a transações no produtor
                producer.InitTransactions(TimeSpan.FromSeconds(5));

                // Inicia uma transação
                producer.BeginTransaction();

                // Envia a mensagem para o tópico especificado
                var result = await producer.ProduceAsync(Topico, new Message<string, Curso>
                {
                    Key = Guid.NewGuid().ToString(), // Chave única da mensagem
                    Value = mensagem // Valor da mensagem, no formato Avro
                });

                // Verifica se a mensagem foi persistida com sucesso
                if (result.Status == PersistenceStatus.Persisted)
                {
                    producer.CommitTransaction(); // Confirma a transação
                    Console.WriteLine($"Mensagem {indice} enviada com sucesso para {result.TopicPartitionOffset}");
                }
                else
                {
                    producer.AbortTransaction(); // Aborta a transação se houver falha
                    Console.WriteLine($"Mensagem {indice} falhou e a transação foi abortada.");
                }
            }
            catch (Exception ex)
            {
                // Trata erros ocorridos durante a produção da mensagem
                Console.Error.WriteLine($"Erro ao produzir mensagem: {ex}");
            }
        }

        /// <summary>
        /// Consome mensagens do Kafka utilizando serialização Avro.
        /// </summary>
        static void ConsumirMensagens(string grupoConsumidor, AutoOffsetReset autoOffsetReset)
        {
            // Configuração do Schema Registry
            var schemaConfig = new SchemaRegistryConfig { Url = ServerAvro };
            using var schemaRegistry = new CachedSchemaRegistryClient(schemaConfig); // Cliente para gerenciar esquemas Avro

            // Configuração do consumidor Kafka
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = BootstrapServer, // Servidor Kafka
                GroupId = grupoConsumidor, // Grupo de consumidor para compartilhamento de mensagens
                AutoOffsetReset = autoOffsetReset, // Ponto de início do consumo (mais recente ou mais antigo)
                EnablePartitionEof = true, // Habilita o controle de fim de partição
                EnableAutoCommit = false, // Desativa commits automáticos
                EnableAutoOffsetStore = false, // Controle manual de offsets
                IsolationLevel = IsolationLevel.ReadCommitted, // Configurar para consumir apenas mensagens confirmadas.
                ClientId = $"{grupoConsumidor}-{Guid.NewGuid().ToString().Substring(0, 5)}" // Identificador único do cliente
            };

            // Criação do consumidor Kafka
            using var consumer = new ConsumerBuilder<string, Curso>(consumerConfig)
                .SetValueDeserializer(new AvroDeserializer<Curso>(schemaRegistry).AsSyncOverAsync()) // Deserializa valores Avro
                .Build();

            consumer.Subscribe(Topico); // Inscreve o consumidor no tópico especificado
            Console.WriteLine($"Consumidor iniciado no grupo {grupoConsumidor}...");

            try
            {
                while (true) // Loop infinito para consumo contínuo
                {
                    var result = consumer.Consume(); // Consome a próxima mensagem disponível

                    if (result.IsPartitionEOF) // Verifica se chegou ao fim da partição
                        continue;

                    // Exibe a mensagem recebida no console
                    Console.WriteLine($"[Grupo: {grupoConsumidor}] Mensagem recebida: {result.Message.Value.descricao}");

                    try
                    {
                        // Processa a mensagem (lógica de negócio pode ser adicionada aqui)
                        Console.WriteLine($"[Grupo: {grupoConsumidor}] Processando mensagem...");

                        // Confirma o processamento bem-sucedido da mensagem
                        consumer.StoreOffset(result); // Armazena o offset manualmente
                        consumer.Commit(result); // Confirma o consumo da mensagem
                    }
                    catch (Exception processEx)
                    {
                        // Trata erros ocorridos durante o processamento da mensagem
                        Console.Error.WriteLine($"Erro ao processar mensagem: {processEx}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Trata erros ocorridos durante o consumo de mensagens
                Console.Error.WriteLine($"Erro no consumidor do grupo {grupoConsumidor}: {ex}");
            }
            finally
            {
                consumer.Close(); // Fecha o consumidor adequadamente
            }
        }

        #region Consumir mensagens mais de um vez
        //static void Consumir(string consumerId, AutoOffsetReset autoOffsetReset)
        //{
        //    var clientId = Guid.NewGuid().ToString().Substring(0, 5);

        //    var conf = new ConsumerConfig
        //    {
        //        ClientId = clientId,
        //        GroupId = consumerId,
        //        BootstrapServers = "localhost:9092",
        //        AutoOffsetReset = autoOffsetReset,
        //        EnablePartitionEof = true,
        //        EnableAutoCommit = false,
        //        EnableAutoOffsetStore = false,

        //        // Configurar para consumir apenas mensagens confirmadas.
        //        IsolationLevel = IsolationLevel.ReadCommitted,
        //    };

        //    using var consumer = new ConsumerBuilder<string, string>(conf).Build();

        //    consumer.Subscribe(Topico);

        //    int Tentativas = 0;

        //    while (true)
        //    {
        //        var result = consumer.Consume();

        //        if (result.IsPartitionEOF)
        //        {
        //            continue;
        //        }

        //        //var messsage = "<< Recebida: \t" + result.Message.Value + $" - {consumerId}-{autoOffsetReset}-{clientId}";
        //        var messsage = "<< Recebida: \t" + result.Message.Value;
        //        Console.WriteLine(messsage);

        //        // Tentar processar mensagem
        //        Tentativas++; 
        //        if (!ProcessarMensagem(result) && Tentativas < 3)
        //        { 
        //            consumer.Seek(result.TopicPartitionOffset);

        //            continue;
        //        }

        //        if(Tentativas > 1)
        //        {
        //            // Publicar mensagem em uma fila para analise!
        //            Console.WriteLine("Enviando mensagem para: DeadLetter");
        //            Tentativas = 0;
        //        }

        //        consumer.Commit(result);
        //        consumer.StoreOffset(result.TopicPartitionOffset); 
        //    }
        //}
        #endregion

    }

}

