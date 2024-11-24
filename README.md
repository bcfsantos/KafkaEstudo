# KafkaEstudo
O que é o Apache Kafka?
O Apache Kafka é uma ferramenta de código aberto usada para lidar com grandes volumes de dados em tempo real. Pense nele como um sistema de correio: ele envia mensagens de um lugar para outro, garantindo que elas cheguem no momento certo. Essas mensagens podem ser eventos, como "uma pessoa fez login no sistema" ou "um pedido foi concluído".

Para que serve o Apache Kafka?
Ele é usado principalmente para:
Transmitir dados em tempo real entre diferentes sistemas ou aplicações.
Armazenar dados temporariamente enquanto são processados.
Criar pipelines de dados: sequências de passos para processar grandes volumes de informação.
Análise em tempo real, como monitorar transações bancárias ou analisar cliques em um site.

Estrutura do Apache Kafka com uma analogia a um condomínio
Imagine que o Apache Kafka é como um condomínio:
Condomínio (Cluster Kafka):
O condomínio representa o cluster Kafka, onde todos os apartamentos (brokers) estão localizados.
Apartamentos (Brokers):
Cada apartamento é um broker, onde os dados (mensagens) são armazenados. Vários apartamentos trabalham juntos para atender os moradores (aplicativos) do condomínio.
Andares (Tópicos):
Os andares do condomínio representam os tópicos. Cada andar pode ter diferentes apartamentos (brokers) que armazenam mensagens sobre um tema específico.
Quartos (Partições):
Dentro de cada apartamento, os quartos são as partições. Cada quarto pode ser acessado individualmente, permitindo que os moradores (produtores e consumidores) interajam com as mensagens de forma paralela.
Moradores (Produtores e Consumidores):
Os moradores do condomínio são os produtores e consumidores. Os produtores são aqueles que trazem novos móveis (mensagens) para os apartamentos, enquanto os consumidores são os que vêm buscar ou utilizar esses móveis.
Visitas (Grupo de Consumidores):
Os visitantes que compartilham a mesma casa (grupo de consumidores) representam como os consumidores podem trabalhar juntos para ler mensagens de um tópico, garantindo que todos tenham acesso sem duplicação.
Síndico (Zookeeper):
O síndico do condomínio seria o Zookeeper, responsável por coordenar as atividades do condomínio, como a gestão de regras e a manutenção das áreas comuns, garantindo que tudo funcione de maneira harmoniosa.


Como realizar o download do Apache Kafka
Acesse o site oficial: Apache Kafka Downloads.
Escolha a versão mais recente.
Clique no link de download e aguarde.

Como instalar o Apache Kafka
Após o download, extraia o arquivo ZIP ou TAR para um diretório local.
Instale o Java (Java Runtime Environment - JRE) no computador, pois o Kafka precisa dele.
Configure as variáveis de ambiente para o Java (se necessário).

Como configurar o Apache Kafka
Caso seja necessário efetuar alguma alteração na configuração padrão, acesse o arquivo server.properties, que fica na pasta config dentro do diretório do Kafka.
Configure a porta padrão (9092) e outros parâmetros, como o local onde os dados serão armazenados.
Inicie o Zookeeper (comando: bin/zookeeper-server-start.sh config/zookeeper.properties).

Em seguida, inicie o Kafka (comando: bin/kafka-server-start.sh config/server.properties).


Como criar, listar e excluir tópicos
Criar: bin/kafka-topics.sh --create --topic [nome_do_topico] --bootstrap-server localhost:9092 --partitions [n] --replication-factor [r]
Listar: bin/kafka-topics.sh --list --bootstrap-server localhost:9092
Excluir: bin/kafka-topics.sh --delete --topic [nome_do_topico] --bootstrap-server localhost:9092


Como criar tópico em uma partição
Adicione --partitions 1 ao comando de criação do tópico.

Como criar tópico em uma partição com replicação
Adicione --replication-factor 2 (ou mais) ao comando de criação. A replicação cria cópias das mensagens para aumentar a segurança.

Como alterar a quantidade de partições
Use:
bin/kafka-topics.sh --alter --topic [nome_do_topico] --partitions [nova_quantidade] --bootstrap-server localhost:9092

Como obter informações de tópicos
Use:
bin/kafka-topics.sh --describe --topic [nome_do_topico] --bootstrap-server localhost:9092

Como produzir e consumir mensagens
Produzir (enviar): bin/kafka-console-producer.sh --topic [nome_do_topico] --bootstrap-server localhost:9092
Consumir (ler): bin/kafka-console-consumer.sh --topic [nome_do_topico] --bootstrap-server localhost:9092 --from-beginning

Confluent
Download
https://docs.confluent.io/platform/current/get-started/platform-quickstart.html
Iniciar Confluent
Obs.: Acessar a pasta do arquivo “.yml” via prompot e executar o comando abaixo:
docker compose -f docker-compose.yml up -d

O Confluent é uma plataforma que fornece ferramentas e serviços para trabalhar com o Apache Kafka, visando facilitar o desenvolvimento, a implementação e a operação de aplicações baseadas em streaming. Aqui estão algumas das principais funcionalidades e propósitos do Confluent:

Kafka como Serviço: 
O Confluent oferece uma versão gerenciada do Kafka, permitindo que empresas utilizem Kafka na nuvem sem se preocupar com a infraestrutura subjacente.

Schema Registry: 
Um componente que permite gerenciar esquemas de dados, garantindo que os produtores e consumidores estejam sempre em conformidade com o formato esperado das mensagens. Isso ajuda a evitar problemas de incompatibilidade.

Kafka Connect: 
Facilita a integração de Kafka com sistemas externos, como bancos de dados, sistemas de arquivos e serviços em nuvem, permitindo a movimentação de dados de e para Kafka de forma simplificada.

KSQL: 
Uma interface SQL para interagir com dados em tempo real dentro do Kafka. Permite fazer consultas e transformações de dados diretamente nas streams, utilizando uma linguagem semelhante ao SQL.

Monitoramento e Gerenciamento:
 Ferramentas que ajudam a monitorar o desempenho do cluster Kafka, identificar gargalos e garantir que tudo esteja funcionando corretamente.

Segurança e Compliance: 
O Confluent oferece recursos adicionais de segurança, como autenticação e criptografia, que são essenciais para ambientes empresariais.
Documentação e Suporte: 
Fornece documentação detalhada e suporte profissional, facilitando a adoção e resolução de problemas.


