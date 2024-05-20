const API_URL = 'https://d5dka4mjuofl7njm3sa7.apigw.yandexcloud.net';

fetch("version")
    .then((res) => res.text())
    .then((text) => {
        const clientVersionElement = document.querySelector('.client-version');
        clientVersionElement.textContent = `web v${text}`;
    })

if (document.cookie.indexOf('user=') === -1) {
    document.cookie = `user=${crypto.randomUUID()}; max-age=86400`;
}
const currentUserId = getCookieValue('user');

const options = {
    mode: "cors",
    cache: "no-cache",
    //credentials: "same-origin",
    headers: {
        "Access-Control-Allow-Origin": "*",
        'Content-Type': 'application/json;charset=utf-8',
        'Accept': 'application/json',
        'X-User-Id': currentUserId
    }
};

const messagesWithMeta = await getMessagesAsync();
setServerMeta(messagesWithMeta.hostName, messagesWithMeta.version);
const messages = messagesWithMeta.messages;
const messagesList = document.querySelector('.messages');

for (const message of messages) {
    const isCurrentUserMessage = currentUserId === message.userId;
    const localTime = new Date(message.send).toLocaleTimeString();
    addMessage(isCurrentUserMessage, message.text, getTimeWithoutSeconds(localTime));
}

const sendButton = document.querySelector('.send-message-button');
sendButton.onclick = handleSendButtonClick

async function handleSendButtonClick() {
    const textInput = document.querySelector("#message");
    const time = new Date();
    await sendMessageAsync(textInput.value, new Date().toISOString());
    textInput.value = '';
    addMessage(true, textInput.value, getTimeWithoutSeconds(time.toLocaleTimeString()));
}

async function getMessagesAsync() {
    const response = await fetch(`${API_URL}/messages`, options);
    return response.json();
}

async function sendMessageAsync(text, time) {
    await fetch(`${API_URL}/messages`, {...options,
        method: 'POST',
        body: JSON.stringify({
            Text: text,
            Send: time
        })
    });
}

function addMessage(isCurrentUserMessage, text, time) {
    const listItem = document.createElement('li');
    const itemContentContainer = document.createElement('div');
    const textElement = document.createElement('div');
    const timeElement = document.createElement('span');

    listItem.className = 'messages-item';
    const ownerClass = isCurrentUserMessage ? ['message-owned'] : ['message-unfamiliar'];
    listItem.classList.add(...ownerClass);
    itemContentContainer.className = 'message-content-container';
    textElement.className = 'message-text';
    timeElement.className = 'message-time';
    textElement.innerText = text;
    timeElement.innerText = time;

    if (isCurrentUserMessage) {
        itemContentContainer.appendChild(textElement);
        itemContentContainer.appendChild(timeElement);
    } else {
        itemContentContainer.appendChild(timeElement);
        itemContentContainer.appendChild(textElement);
    }

    listItem.appendChild(itemContentContainer)
    messagesList.appendChild(listItem);
}

function setServerMeta(host, version) {
    const versionElement = document.querySelector('.server-version');
    const replicaElement = document.querySelector('.server-replica-name');
    versionElement.textContent = `server v${version}`;
    replicaElement.textContent = `server container: ${host}`;
}

function getCookieValue(name)
{
    const regex = new RegExp(`(^| )${name}=([^;]+)`)
    const match = document.cookie.match(regex)
    if (match) {
        return match[2]
    }
}

function getTimeWithoutSeconds(time) {
    const slicedTime = time.split(':');
    return slicedTime[0] + ':' + slicedTime[1];
}
