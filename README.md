
# Pontos importantes:
  - Por implementar services na camada de domínio seria necessário alguma implementação de notificação para retornar mais especificamente para a api porque alguma requisição deu errada. Algumas travas que eu acabei colocando estão meio fora do local correto pra poder dar uma resposta mais especifica na hora de testar. Para validação das entidades seria ainda mais necessário esta implementação.
  - Em vez de perguntar se a pessoa é vegana, eu também teria deixado isso salvo no próprio usuário, teria faciliado os endpoints de aceite e recusa, e quando uma pessoa cancelasse não seria necessário perguntar novamente se ela é ou não.
  - Como é fixo apenas carnes e vegetais eu teria feito apenas variáveis para controlar isso, mas como no exercicio pede lista, tentei implementando um dictionary, e com pouco esforço seria possivel adicionar itens dinamicamente. Ou trocar para um Enumerable com uma classe específica de item.
  - Não tem muita especificação sobre impedir o uso de endpoints então tomei a liberdade de bloquear alguns que atrapalhavam a execução das regras, como aceitar um mesmo convite varias vezes, ou a moderação repetidas vezes de um mesmo evento (O que deixou mais evidente a questão das notificações).
    - Daria por exemplo para ter permitido moderar mais de uma vez para aceito, e voltar os status corretos para os convites. 
  

<br/>
<br/>
<br/>
<br/>
<br/>
<br/>

## Deixei um comentário no metodo referente a lista de compras. <br/>Não entendi muito bem o requisito, mas acredito ter cumprido o necessário também

#### "Deve ser possível ver a lista de compras informando a quantidade de carne e vegetais que devem ser comprados. A quantidade deve ser exibida em quilos. 👎🏽"

``//Pela descrição da tarefa da lista de compras parecem querer avaliar como o candidato realizaria a busca tanto por id``<br/>
``//quanto por outros parametros (uma consulta com filtros dinamicos),``<br/>
``//se a busca fosse somente por id não seria tão estranho o código para pegar a lista de compras, não entendi se era necessário esse filtro a mais.``<br/>
``//Eu também normalmente não colocaria isso por linq e sim na consulta do banco de dados pelo repositorio``<br/>
``//mas não encontrei uma forma de fazer isso sem passar por cima da interface de IEventStore utilizada na definição das dependências``<br/>
``//E a forma como o banco fica estruturado também dificulta essa outra abordagem``<br/>
