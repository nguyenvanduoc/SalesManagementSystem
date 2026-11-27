# CLAUDE.md

## Quy định Git (do chủ dự án yêu cầu)

- **Mặc định code trực tiếp trên nhánh `main`**: commit và push thẳng lên `main`,
  không tạo nhánh `claude/...` riêng và không cần tạo Pull Request,
  trừ khi chủ dự án yêu cầu khác trong cuộc trò chuyện.
- Trước khi code luôn `git pull origin main` để lấy code mới nhất.

## Quy chuẩn dự án

Đọc và tuân thủ `SalesManagementSystem/AGENTS.md`, `SalesManagementSystem/UI_STANDARD.md`,
`SalesManagementSystem/ARCHITECTURE_STANDARD.md`, `SalesManagementSystem/DATABASE_STANDARD.md`.

## Giải thích sau khi code

Code xong phải giải thích rõ công dụng của từng API / hàm đã thêm hoặc sửa.
