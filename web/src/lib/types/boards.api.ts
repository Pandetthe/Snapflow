export namespace GetBoardsResponse {
  export interface UserDto {
    id: number;
    userName: string;
  }

  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    createdAt: string;
    createdBy: UserDto;
    updatedAt: string | null;
    updatedBy: UserDto | null;
  }
}

export namespace GetBoardByIdResponse {
  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    swimlanes: SwimlaneDto[];
  }

  export interface UserDto {
    id: number;
    userName: string;
  }

  export interface SwimlaneDto {
    id: number;
    title: string;
    height: number | null;
    rank: string;
    lists: ListDto[];
  }

  export interface ListDto {
    id: number;
    title: string;
    rank: string;
    cards: CardDto[];
  }

  export interface CardDto {
    id: number;
    title: string;
    description: string;
    rank: string;
    createdAt: string;
    createdBy: UserDto;
    updatedAt: string | null;
    updatedBy: UserDto | null;
  }
}
