// Изменяемые константы — при необходимости подкорректировать под бекенд
export const CONFIRMATION_TYPES = {
  RESPOND_TO_MINOR_TASK: 'Respond to minor task',
};

export const CONFIRMATION_DEFAULT_EXPIRATION_HOURS = 72;

// Бэкенд использует "Created" как начальный статус (не "Pending").
// Обе строки обозначают одно — "ждёт решения".
export const STATUS_LABELS = {
  created:  'Ожидает',
  pending:  'Ожидает',
  accepted: 'Принята',
  rejected: 'Отклонена',
  revoked:  'Отозвана',
  expired:  'Истекла',
};
