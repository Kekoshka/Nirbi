// Безопасный разбор metaData подтверждения.
// Из Gateway-агрегатора и из БД ConfirmationService metaData может прийти как
// JSON-строка; из SignalR-payload иногда как уже распарсенный объект.
// Возвращаем всегда объект (пустой, если распарсить нельзя).
export function parseMetaData(meta) {
  if (!meta) return {};
  if (typeof meta === 'object') return meta;
  if (typeof meta === 'string') {
    try { return JSON.parse(meta) || {}; }
    catch { return {}; }
  }
  return {};
}
