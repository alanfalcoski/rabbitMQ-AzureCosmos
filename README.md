# rabbitMQ-AzureCosmos
Aplicação utilizando a estratégia Remote Procedure Call (RPC) - RabbitMQ

Escolhi a estratégia RPC para implantar o fluxo enviado.
![image](https://user-images.githubusercontent.com/33583484/164916923-a6b386c8-1a4e-449b-8d96-da095dee6f6f.png)

Pattern:
![image](https://user-images.githubusercontent.com/33583484/164916909-28b9c83b-e6e9-4afa-8334-309dfdd78ce9.png)

O ambiente deve conter dotnetCore 6 e o rabbitMQ executando na porta padrão.

Para iniciar o projeto, basta clonar o repositório e iniciar a aplicações de console com o comando "dotnet run".

Iniciar primeiramente o projeto RPCServer, que ficará ouvindo por solicitações enviadas para filas do RabbitMQ. Ao Executá-lo uma fila chamada "forecast_queue" deverá ser criada, na aplicação de console irá aparecer:

![image](https://user-images.githubusercontent.com/33583484/164917086-d5764e39-b301-435b-a9d4-cf3800f21e13.png)


Agora, é necessário rodar o projeto RPCClient, este projeto irá escutar chamada na interface de api no endereço "http://localhost:8081/api/WeatherForecast". Na tela irá aparecer (Obs.: se tiver outra aplicação executando nesta porta será necessário trocar):
![image](https://user-images.githubusercontent.com/33583484/164917296-b09a136c-b1b2-45b7-8c92-bbfce88eb0c8.png)

Após, devemos chamar este endereço passando o parâmetro City, exemplo:
"http://localhost:8081/api/WeatherForecast?City=Curitiba".

![image](https://user-images.githubusercontent.com/33583484/164918411-d5d49e91-e71f-4799-9093-7fa89da98682.png)

Então, é criada uma nova fila no Rabbit, esperando um retorno do servidor:
![image](https://user-images.githubusercontent.com/33583484/164919457-6020275b-e001-4863-ad7a-5a528afc58d3.png)

O servidor recebe a requisição e responde ao Rabbit, que redireciona a resposta ao Cliente:
![image](https://user-images.githubusercontent.com/33583484/164920104-8275ddaf-617b-49c2-938c-8f253ffc9b53.png)

Por fim, irá criar o registro no CosmosDB e registrar o id em tela. Bem como os valores do objeto gravado:
![image](https://user-images.githubusercontent.com/33583484/164920988-279ea6f0-3e03-4348-9dd9-5e5968401590.png)

![image](https://user-images.githubusercontent.com/33583484/164923524-590791db-679b-49d8-9f98-a47f85477125.png)


