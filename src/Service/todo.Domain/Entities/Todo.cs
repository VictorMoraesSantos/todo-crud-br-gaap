namespace task_crud.Domain.Entities
{
    public class Todo
    {
        public int UserId { get; private set; }
        public int Id { get; private set; }
        public string Title { get; private set; }
        public bool Completed { get; private set; }

        public Todo(int userId, string title, bool completed)
        {
            UserId = userId;
            Title = title;
            Completed = completed;
        }

        public void MarkAsCompleted()
        {
            Completed = true;
        }

        public void MarkAsIncomplete()
        {
            if (!Completed) return;

            Completed = false;
        }

        public void SetTitle(string title)
        {
            if (Title == title) return;
            Title = title;
        }

        public void SetUserId(int userId)
        {
            if (UserId == userId) return;
            UserId = userId;
        }

        public void Update(int userId, string title, bool completed)
        {
            SetUserId(userId);
            SetTitle(title);

            if (completed)
                MarkAsCompleted();
            else
                MarkAsIncomplete();

        }
    }
}
