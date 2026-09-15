/** A plain message for the user: something happened that they should know about, but nothing failed. */
export interface AppNotice {
  title: string;
  message: string;
}

class NoticeState {
  notices = $state<AppNotice[]>([]);

  add(title: string, message: string) {
    this.notices = [...this.notices, { title, message }];
  }

  reset() {
    this.notices = [];
  }
}

export const noticeStore = new NoticeState();
