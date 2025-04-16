namespace backend_tetris.DTOs;

public record GameStateDto(int yourScore, int enemyScore, string enemyUsername, int timeToEndInSeconds);
