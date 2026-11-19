# СУР — система управления рисками (Docker-демо)

Прототип системы управления рисками для проверки: реестр рисков, оценка, меры, согласование, Audit Snapshot (PDF/CSV).

## Быстрый запуск

Нужен **Docker Desktop** (Linux containers).

Подробные команды — в файле [`ЗАПУСК.md`](./ЗАПУСК.md).

Кратко:

```powershell
docker build -t sur:latest .
docker run --rm -p 8080:8080 -v sur-data:/app/App_Data sur:latest
```

Сайт: http://127.0.0.1:8080/

## Вход

Пароль у всех демо-учёток: **`sur-demo`**

| Email | Роль |
| --- | --- |
| `admin@sur.local` | Администратор |
| `owner@sur.local` | Владелец процесса |
| `director@sur.local` | Директор |
| `employee@sur.local` | Сотрудник |
| `ceo@sur.local` | CEO |
| `specialist@sur.local` | Специалист по рискам |
| `finance@sur.local` | Владелец / директор (финансы) |

Самостоятельной регистрации нет.
