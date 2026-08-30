const handForm = document.querySelector('#hand-form');
const texasForm = document.querySelector('#texas-form');
const handResult = document.querySelector('#hand-result');
const texasResult = document.querySelector('#texas-result');

handForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const cards = document.querySelector('#cards').value;
  const response = await fetch('/api/hand', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({ cards })
  });

  const payload = await response.json();
  handResult.textContent = response.ok
    ? JSON.stringify(payload, null, 2)
    : `Erro: ${payload.error ?? 'Falha ao avaliar a mão.'}`;
});

texasForm.addEventListener('submit', async (event) => {
  event.preventDefault();

  const players = document.querySelector('#players').value;
  const response = await fetch('/api/texas-holdem', {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({ players })
  });

  const payload = await response.json();
  texasResult.textContent = response.ok
    ? JSON.stringify(payload, null, 2)
    : `Erro: ${payload.error ?? 'Falha ao executar o Texas Holdem.'}`;
});
