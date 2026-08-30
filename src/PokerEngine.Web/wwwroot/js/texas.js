const texasForm = document.querySelector('#texas-form');
const communityCards = document.querySelector('#community-cards');
const winnerSummary = document.querySelector('#winner-summary');
const playerCards = document.querySelector('#player-cards');
const bestHands = document.querySelector('#best-hands');
const resultRaw = document.querySelector('#result-raw');

function renderTexasError(message) {
  renderCardRow(communityCards, '');
  winnerSummary.textContent = message;
  playerCards.innerHTML = '';
  bestHands.innerHTML = '';
  resultRaw.textContent = message;
}

function renderPlayerList(holeCards) {
  if (!holeCards || Object.keys(holeCards).length === 0) {
    playerCards.innerHTML = '<div class="list-item">-</div>';
    return;
  }

  const players = Object.entries(holeCards).map(([player, cards]) => {
    const isWinner = Number(player) === Number(document.querySelector('#winner-summary')?.dataset?.winnerPlayer ?? -1);
    const playerClass = isWinner ? 'player-card-box winner-box' : 'player-card-box';

    return `<div class="${playerClass}">
      <div class="player-card-title">Jogador ${player}${isWinner ? ' • Campeão' : ''}</div>
      <div class="card-row">${cards.map((card) => createCardToken(card).outerHTML).join('')}</div>
    </div>`;
  });

  playerCards.innerHTML = players.join('');
}

function renderBestHands(entries) {
  if (!entries || entries.length === 0) {
    bestHands.innerHTML = '<div class="list-item">-</div>';
    return;
  }

  const winnerPlayer = Number(document.querySelector('#winner-summary')?.dataset?.winnerPlayer ?? -1);

  const markup = entries.map((entry, index) => {
    const title = entry.player === 0 ? 'Mesa' : `Jogador ${entry.player}`;
    const ranking = entry.ranking || '-';
    const description = entry.description || '-';
    const handText = description.includes('[') && description.includes(']')
      ? description.slice(0, description.lastIndexOf('[')).trim()
      : description;
    const handCards = description.includes('[') && description.includes(']')
      ? description.slice(description.indexOf('[') + 1, description.lastIndexOf(']'))
      : '';
    const medalClass = index === 0 ? 'podium-gold' : index === 1 ? 'podium-silver' : index === 2 ? 'podium-bronze' : '';
    const playerClass = index < 3 ? `player-card-box ${medalClass}`.trim() : 'player-card-box';
    const winnerBadge = entry.player === winnerPlayer ? ' • Campeão' : '';

    return `
      <div class="${playerClass}">
        <div class="player-card-title">${title}${winnerBadge}</div>
        <div class="list-item"><strong>Ranking:</strong> ${ranking}</div>
        <div class="list-item"><strong>Mão:</strong> ${handText}</div>
        ${handCards
          ? `<div class="card-row">${handCards.split(',').map((card) => createCardToken(card.trim()).outerHTML).join('')}</div>`
          : ''}
      </div>
    `;
  }).join('');

  bestHands.innerHTML = markup;
}

function renderTexasSuccess(payload) {
  renderCardRow(communityCards, payload.communityCards?.join(', ') ?? '');

  const winnerPlayer = payload.winner ? payload.winner.player : null;
  winnerSummary.dataset.winnerPlayer = String(winnerPlayer ?? '');
  winnerSummary.textContent = payload.winner ? `${payload.winner.player === 0 ? 'Mesa' : `Jogador ${payload.winner.player}`} — ${payload.winner.ranking}` : '-';

  renderPlayerList(payload.holeCards);
  renderBestHands(payload.bestHands ?? []);
  resultRaw.textContent = JSON.stringify(payload, null, 2);
}

if (texasForm) {
  texasForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const players = document.querySelector('#players').value;
    const response = await fetch('/api/texas-holdem', {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams({ players })
    });

    const payload = await response.json();

    if (!response.ok) {
      renderTexasError(payload.error ?? 'Falha ao executar o Texas Holdem.');
      return;
    }

    renderTexasSuccess(payload);
  });
}
