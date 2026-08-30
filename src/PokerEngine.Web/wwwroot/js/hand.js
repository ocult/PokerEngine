const handForm = document.querySelector('#hand-form');
const handResult = document.querySelector('#hand-result');

if (handForm) {
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
}
