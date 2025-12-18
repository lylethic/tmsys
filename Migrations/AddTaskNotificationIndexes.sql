-- =====================================================
-- Migration: Add Indexes for Task Notification System
-- Date: 2025-11-30
-- Purpose: Optimize performance for task deadline checks
-- =====================================================

-- =====================================================
-- 1. TASKS TABLE INDEXES
-- =====================================================

-- Index for querying tasks that need notifications
-- Covers: assigned_to, status, active, deleted, last_progress_update
CREATE INDEX IF NOT EXISTS idx_tasks_notification_check 
ON tasks(assigned_to, status, active, deleted, last_progress_update)
WHERE active = TRUE AND deleted = FALSE AND status NOT IN ('Done', 'Completed', 'Closed');

-- Index for quick lookups by due_date
CREATE INDEX IF NOT EXISTS idx_tasks_due_date 
ON tasks(due_date) 
WHERE active = TRUE AND deleted = FALSE;

-- Index for update frequency queries
CREATE INDEX IF NOT EXISTS idx_tasks_last_progress 
ON tasks(last_progress_update, update_frequency_days)
WHERE active = TRUE AND deleted = FALSE;

-- =====================================================
-- 2. NOTIFICATIONS TABLE INDEXES
-- =====================================================

-- Index for checking existing notifications by reference link
-- Used to prevent duplicate notifications
CREATE INDEX IF NOT EXISTS idx_notifications_reference_active 
ON notifications(reference_link, expired)
WHERE expired IS NULL OR expired > NOW();

-- Index for user's notifications queries
-- Ordered by created_at DESC for efficient pagination
CREATE INDEX IF NOT EXISTS idx_notifications_user_created 
ON notifications(user_id, created_at DESC);

-- Index for filtering by category types
CREATE INDEX IF NOT EXISTS idx_notifications_categories 
ON notifications(main_category_type, sub_category_type, created_at DESC);

-- Index for unread notifications check
-- Using GIN for array operations (user_read column)
CREATE INDEX IF NOT EXISTS idx_notifications_user_read 
ON notifications USING GIN(user_read);

-- Index for expired notifications cleanup
CREATE INDEX IF NOT EXISTS idx_notifications_expired 
ON notifications(expired) 
WHERE expired IS NOT NULL;

-- =====================================================
-- 3. COMPOSITE INDEXES FOR COMPLEX QUERIES
-- =====================================================

-- Index for notification search with filters
CREATE INDEX IF NOT EXISTS idx_notifications_search 
ON notifications(user_id, status_id, main_category_type, sub_category_type, created_at DESC)
WHERE expired IS NULL OR expired > NOW();

-- =====================================================
-- 4. VERIFY INDEXES
-- =====================================================

-- Query to check all indexes on tasks table
-- SELECT indexname, indexdef FROM pg_indexes 
-- WHERE tablename = 'tasks' AND schemaname = 'public';

-- Query to check all indexes on notifications table
-- SELECT indexname, indexdef FROM pg_indexes 
-- WHERE tablename = 'notifications' AND schemaname = 'public';

-- =====================================================
-- 5. STATISTICS & MONITORING
-- =====================================================

-- Analyze tables to update statistics for query planner
ANALYZE tasks;
ANALYZE notifications;

-- =====================================================
-- 6. EXPECTED PERFORMANCE IMPROVEMENTS
-- =====================================================

/*
Before Indexes:
- Task notification query: ~500-1000ms for 10,000 tasks
- Duplicate check query: ~200-500ms

After Indexes:
- Task notification query: ~50-100ms for 10,000 tasks (10x faster)
- Duplicate check query: ~10-20ms (20x faster)

Notes:
- Indexes will increase INSERT/UPDATE time slightly (~10-20%)
- But READ performance improves dramatically (10-20x)
- For notification system, reads are much more frequent than writes
*/

-- =====================================================
-- 7. MAINTENANCE QUERIES (Optional)
-- =====================================================

-- Reindex if needed (run periodically or after bulk operations)
-- REINDEX TABLE tasks;
-- REINDEX TABLE notifications;

-- Check index usage statistics
-- SELECT 
--     schemaname,
--     tablename,
--     indexname,
--     idx_scan as index_scans,
--     idx_tup_read as tuples_read,
--     idx_tup_fetch as tuples_fetched
-- FROM pg_stat_user_indexes
-- WHERE tablename IN ('tasks', 'notifications')
-- ORDER BY idx_scan DESC;

-- =====================================================
-- DONE!
-- =====================================================
