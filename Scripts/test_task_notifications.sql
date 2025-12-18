-- =====================================================
-- TEST SCRIPT - Task Deadline Notification System
-- Date: 2025-11-30
-- Purpose: Kịch bản test cho Hangfire notification
-- =====================================================

-- =====================================================
-- CHUẨN BỊ: Lấy User ID để test
-- =====================================================

-- Lấy danh sách users
SELECT id, email, full_name FROM users WHERE active = TRUE LIMIT 10;

-- Chọn 1 user_id để test (thay thế <YOUR_USER_ID> bên dưới)
-- Example: 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

-- =====================================================
-- SCENARIO 1: Task ĐẾN HẠN HÔM NAY
-- =====================================================

-- Task với update_frequency = 1 ngày, last update = hôm qua
-- → Hôm nay là deadline

INSERT INTO tasks (
    id,
    project_id,
    name,
    description,
    assigned_to,
    status,
    due_date,
    priority,
    update_frequency_days,
    last_progress_update,
    created,
    updated,
    created_by,
    updated_by,
    deleted,
    active
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM projects WHERE active = TRUE LIMIT 1), -- Lấy 1 project bất kỳ
    'TEST - Task đến hạn hôm nay',
    'Task này đến hạn cập nhật tiến độ hôm nay. Dùng để test notification.',
    '<YOUR_USER_ID>'::uuid, -- THAY BẰNG USER ID THỰC
    'In Progress',
    CURRENT_DATE + INTERVAL '7 days', -- Due date (không quan trọng lắm)
    2, -- Priority: Medium
    1, -- Update mỗi 1 ngày
    CURRENT_DATE - INTERVAL '1 day', -- Last update = hôm qua
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    '<YOUR_USER_ID>'::uuid, -- Created by
    '<YOUR_USER_ID>'::uuid, -- Updated by
    FALSE,
    TRUE
);

-- =====================================================
-- SCENARIO 2: Task QUÁ HẠN 3 NGÀY
-- =====================================================

-- Task với update_frequency = 7 ngày, last update = 10 ngày trước
-- → Quá hạn 3 ngày (10 - 7 = 3)

INSERT INTO tasks (
    id,
    project_id,
    name,
    description,
    assigned_to,
    status,
    due_date,
    priority,
    update_frequency_days,
    last_progress_update,
    created,
    updated,
    created_by,
    updated_by,
    deleted,
    active
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM projects WHERE active = TRUE LIMIT 1),
    'TEST - Task quá hạn 3 ngày',
    'Task này đã quá hạn cập nhật 3 ngày. Dùng để test notification overdue.',
    '<YOUR_USER_ID>'::uuid,
    'In Progress',
    CURRENT_DATE + INTERVAL '7 days',
    3, -- Priority: High
    7, -- Update mỗi 7 ngày
    CURRENT_DATE - INTERVAL '10 days', -- Last update = 10 ngày trước
    CURRENT_TIMESTAMP - INTERVAL '10 days',
    CURRENT_TIMESTAMP,
    '<YOUR_USER_ID>'::uuid,
    '<YOUR_USER_ID>'::uuid,
    FALSE,
    TRUE
);

-- =====================================================
-- SCENARIO 3: Task QUÁ HẠN 1 NGÀY
-- =====================================================

INSERT INTO tasks (
    id,
    project_id,
    name,
    description,
    assigned_to,
    status,
    due_date,
    priority,
    update_frequency_days,
    last_progress_update,
    created,
    updated,
    created_by,
    updated_by,
    deleted,
    active
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM projects WHERE active = TRUE LIMIT 1),
    'TEST - Task quá hạn 1 ngày',
    'Task này quá hạn 1 ngày. Test notification.',
    '<YOUR_USER_ID>'::uuid,
    'In Progress',
    CURRENT_DATE + INTERVAL '5 days',
    2,
    5, -- Update mỗi 5 ngày
    CURRENT_DATE - INTERVAL '6 days', -- Last update = 6 ngày trước (6-5=1 ngày quá hạn)
    CURRENT_TIMESTAMP - INTERVAL '6 days',
    CURRENT_TIMESTAMP,
    '<YOUR_USER_ID>'::uuid,
    '<YOUR_USER_ID>'::uuid,
    FALSE,
    TRUE
);

-- =====================================================
-- SCENARIO 4: Task CHƯA ĐẾN HẠN (Không notification)
-- =====================================================

-- Task này không tạo notification vì chưa đến hạn

INSERT INTO tasks (
    id,
    project_id,
    name,
    description,
    assigned_to,
    status,
    due_date,
    priority,
    update_frequency_days,
    last_progress_update,
    created,
    updated,
    created_by,
    updated_by,
    deleted,
    active
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM projects WHERE active = TRUE LIMIT 1),
    'TEST - Task chưa đến hạn',
    'Task này update hôm qua, frequency 7 ngày → chưa đến hạn.',
    '<YOUR_USER_ID>'::uuid,
    'In Progress',
    CURRENT_DATE + INTERVAL '10 days',
    1,
    7, -- Update mỗi 7 ngày
    CURRENT_DATE - INTERVAL '1 day', -- Mới update hôm qua
    CURRENT_TIMESTAMP - INTERVAL '1 day',
    CURRENT_TIMESTAMP,
    '<YOUR_USER_ID>'::uuid,
    '<YOUR_USER_ID>'::uuid,
    FALSE,
    TRUE
);

-- =====================================================
-- SCENARIO 5: Task ĐÃ HOÀN THÀNH (Không notification)
-- =====================================================

-- Status = 'Done' → không tạo notification

INSERT INTO tasks (
    id,
    project_id,
    name,
    description,
    assigned_to,
    status,
    due_date,
    priority,
    update_frequency_days,
    last_progress_update,
    created,
    updated,
    created_by,
    updated_by,
    deleted,
    active
) VALUES (
    gen_random_uuid(),
    (SELECT id FROM projects WHERE active = TRUE LIMIT 1),
    'TEST - Task đã hoàn thành',
    'Task này status Done nên không thông báo dù quá hạn.',
    '<YOUR_USER_ID>'::uuid,
    'Done', -- ✅ Đã hoàn thành
    CURRENT_DATE,
    1,
    7,
    CURRENT_DATE - INTERVAL '20 days', -- Quá hạn 13 ngày nhưng không thông báo
    CURRENT_TIMESTAMP - INTERVAL '20 days',
    CURRENT_TIMESTAMP,
    '<YOUR_USER_ID>'::uuid,
    '<YOUR_USER_ID>'::uuid,
    FALSE,
    TRUE
);

-- =====================================================
-- KIỂM TRA: Xem tasks vừa tạo
-- =====================================================

SELECT 
    id,
    name,
    status,
    assigned_to,
    update_frequency_days,
    last_progress_update,
    -- Tính next update due date
    (last_progress_update + (update_frequency_days * INTERVAL '1 day'))::date as next_update_due,
    -- Tính số ngày quá hạn
    (CURRENT_DATE - (last_progress_update + (update_frequency_days * INTERVAL '1 day'))::date) as days_overdue,
    active,
    deleted
FROM tasks
WHERE name LIKE 'TEST -%'
ORDER BY last_progress_update;

-- =====================================================
-- CÁCH TEST
-- =====================================================

/*
BƯỚC 1: Chạy script này (thay <YOUR_USER_ID> bằng user ID thực)
---------------------------------------------------------------
psql -U postgres -d tms_database -f test_task_notifications.sql

HOẶC copy từng câu INSERT vào pgAdmin/DBeaver


BƯỚC 2: Trigger Hangfire job manually
--------------------------------------
Method 1 - API:
curl -X POST http://localhost:5000/tms/api/v1/jobs/trigger-task-notifications

Method 2 - Đợi job tự chạy (mỗi 30 phút)


BƯỚC 3: Check notifications được tạo
-------------------------------------
*/

SELECT 
    n.id,
    n.summary,
    n.details,
    n.user_id,
    n.sub_category_type,
    n.main_category_type,
    n.reference_link,
    n.created_at,
    n.expired,
    array_length(n.user_read, 1) as readers_count
FROM notifications n
WHERE n.created_at > CURRENT_TIMESTAMP - INTERVAL '1 hour'
  AND n.reference_link LIKE '%tasks%'
ORDER BY n.created_at DESC;

-- =====================================================
-- EXPECTED RESULTS
-- =====================================================

/*
Sau khi trigger job, bạn sẽ thấy:

1. ✅ 3 notifications được tạo:
   - "TEST - Task đến hạn hôm nay" → sub_category_type = 301 (TASK_DEADLINE)
   - "TEST - Task quá hạn 3 ngày" → sub_category_type = 302 (TASK_OVERDUE)
   - "TEST - Task quá hạn 1 ngày" → sub_category_type = 302 (TASK_OVERDUE)

2. ❌ 2 notifications KHÔNG được tạo:
   - "TEST - Task chưa đến hạn" → Chưa đến deadline
   - "TEST - Task đã hoàn thành" → Status = Done

3. 🔔 SignalR push notification đến user (check console hoặc test-client.html)

4. 📊 Logs hiển thị:
   [TaskNotification] Found 3 tasks requiring notification. Processing...
   [TaskNotification] Successfully created and sent 3 notifications in 0.XX s
*/

-- =====================================================
-- CHECK SIGNALR (Frontend)
-- =====================================================

/*
Mở browser: http://localhost:5000/tms/api/test-client.html

1. Connect to SignalR hub
2. Trigger job: POST /v1/jobs/trigger-task-notifications
3. Xem notifications xuất hiện real-time
*/

-- =====================================================
-- CLEANUP: Xóa test data sau khi test xong
-- =====================================================

-- Xóa test tasks
DELETE FROM tasks WHERE name LIKE 'TEST -%';

-- Xóa test notifications
DELETE FROM notifications 
WHERE details LIKE '%TEST -%';

-- Verify đã xóa
SELECT COUNT(*) FROM tasks WHERE name LIKE 'TEST -%';
SELECT COUNT(*) FROM notifications WHERE details LIKE '%TEST -%';

-- =====================================================
-- ADVANCED: Test với nhiều users
-- =====================================================

-- Tạo tasks cho nhiều users khác nhau
DO $$
DECLARE
    user_record RECORD;
    project_id uuid;
BEGIN
    -- Lấy 1 project để dùng chung
    SELECT id INTO project_id FROM projects WHERE active = TRUE LIMIT 1;
    
    -- Loop qua 5 users đầu tiên
    FOR user_record IN 
        SELECT id, email FROM users WHERE active = TRUE LIMIT 5
    LOOP
        -- Task đến hạn cho mỗi user
        INSERT INTO tasks (
            id, project_id, name, description, assigned_to, status,
            update_frequency_days, last_progress_update,
            created, updated, created_by, updated_by, deleted, active
        ) VALUES (
            gen_random_uuid(),
            project_id,
            'TEST BULK - Task for ' || user_record.email,
            'Bulk test task',
            user_record.id,
            'In Progress',
            1, -- 1 day
            CURRENT_DATE - INTERVAL '1 day',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            user_record.id,
            user_record.id,
            FALSE,
            TRUE
        );
    END LOOP;
    
    RAISE NOTICE 'Created test tasks for 5 users';
END $$;

-- Check tasks vừa tạo
SELECT 
    t.name,
    u.email as assigned_user,
    t.update_frequency_days,
    t.last_progress_update
FROM tasks t
JOIN users u ON t.assigned_to = u.id
WHERE t.name LIKE 'TEST BULK -%'
ORDER BY u.email;

-- Cleanup bulk test
-- DELETE FROM tasks WHERE name LIKE 'TEST BULK -%';

-- =====================================================
-- MONITORING QUERIES
-- =====================================================

-- 1. Tổng số tasks cần thông báo HIỆN TẠI
SELECT COUNT(*) as tasks_requiring_notification
FROM tasks
WHERE active = TRUE 
  AND deleted = FALSE 
  AND status NOT IN ('Done', 'Completed', 'Closed')
  AND (last_progress_update + (update_frequency_days * INTERVAL '1 day')) <= (CURRENT_DATE + INTERVAL '1 day');

-- 2. Phân tích chi tiết
SELECT 
    CASE 
        WHEN (CURRENT_DATE - (last_progress_update + (update_frequency_days * INTERVAL '1 day'))::date) > 0 
        THEN 'OVERDUE'
        ELSE 'DUE_TODAY'
    END as notification_type,
    COUNT(*) as count,
    AVG(CURRENT_DATE - (last_progress_update + (update_frequency_days * INTERVAL '1 day'))::date) as avg_days_overdue
FROM tasks
WHERE active = TRUE 
  AND deleted = FALSE 
  AND status NOT IN ('Done')
  AND (last_progress_update + (update_frequency_days * INTERVAL '1 day')) <= (CURRENT_DATE + INTERVAL '1 day')
GROUP BY notification_type;

-- 3. Top users có nhiều tasks quá hạn nhất
SELECT 
    u.email,
    u.full_name,
    COUNT(*) as overdue_tasks,
    AVG(CURRENT_DATE - (t.last_progress_update + (t.update_frequency_days * INTERVAL '1 day'))::date) as avg_days_overdue
FROM tasks t
JOIN users u ON t.assigned_to = u.id
WHERE t.active = TRUE 
  AND t.deleted = FALSE 
  AND t.status NOT IN ('Done')
  AND (t.last_progress_update + (t.update_frequency_days * INTERVAL '1 day')) < CURRENT_DATE
GROUP BY u.id, u.email, u.full_name
ORDER BY overdue_tasks DESC
LIMIT 10;

-- 4. Notifications created trong 24h qua
SELECT 
    DATE_TRUNC('hour', created_at) as hour,
    COUNT(*) as notifications_created
FROM notifications
WHERE created_at > CURRENT_TIMESTAMP - INTERVAL '24 hours'
  AND reference_link LIKE '%tasks%'
GROUP BY hour
ORDER BY hour DESC;

-- =====================================================
-- DONE! 
-- =====================================================

-- Chúc bạn test thành công! 🚀
-- 
-- Questions?
-- 1. Check logs: grep "TaskNotification" logs/app.log
-- 2. Check Hangfire Dashboard: http://localhost:5000/tms/hangfire
-- 3. Check notifications table: SELECT * FROM notifications ORDER BY created_at DESC LIMIT 10;
